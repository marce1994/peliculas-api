using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PeliculasApi.Controllers;
using PeliculasApi.DTOs;
using PeliculasApi.Entidades;
using PeliculasApi.Servicios;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PeliculasApi.Tests.PruebasUnitarias
{
    [TestClass]
    public class ActoresControllerTests : BasePruebas
    {
        [TestMethod]
        public async Task ObtenerPersonasPaginadas()
        {
            var nombreDb = Guid.NewGuid().ToString();
            var contexto = ConstruirContexto(nombreDb);
            var mapper = ConfigurarAutoMapper();

            contexto.Actores.Add(new Actor() { Nombre = "Actor 1" });
            contexto.Actores.Add(new Actor() { Nombre = "Actor 2" });
            contexto.Actores.Add(new Actor() { Nombre = "Actor 3" });
            await contexto.SaveChangesAsync();

            var contexto2 = ConstruirContexto(nombreDb);

            var controller = new ActoresController(contexto2, mapper, null);
            controller.ControllerContext.HttpContext = new DefaultHttpContext(); // Contexto default para trabajar, ya que no estamos en un contexto http

            var pagina1 = await controller.Get(new PaginacionDto() { Pagina = 1, CantidadRegistrosPorPagina = 2 });
            var actoresPagina1 = pagina1.Value;
            Assert.AreEqual(2, actoresPagina1.Count);

            controller.ControllerContext.HttpContext = new DefaultHttpContext(); // Reseteo httpcontext para que sea como una nueva request

            var pagina2 = await controller.Get(new PaginacionDto() { Pagina = 2, CantidadRegistrosPorPagina = 2 });
            var actoresPagina2 = pagina2.Value;
            Assert.AreEqual(1, actoresPagina2.Count);

            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var pagina3 = await controller.Get(new PaginacionDto() { Pagina = 3, CantidadRegistrosPorPagina = 2 });
            var actoresPagina3 = pagina3.Value;
            Assert.AreEqual(0, actoresPagina3.Count);
        }

        [TestMethod]
        public async Task CrearActorSinFoto()
        {
            var nombreDb = Guid.NewGuid().ToString();
            var contexto = ConstruirContexto(nombreDb);
            var mapper = ConfigurarAutoMapper();

            var actor = new ActorCreacionDto() { Nombre = "Pablo", FechaNacimiento = DateTime.Now };

            var mock = new Mock<IAlmacenadorArchivos>();
            mock.Setup(x => x.GuardarArchivo(null, null, null, null)).Returns(Task.FromResult("url")); // Configurar mock

            var controller = new ActoresController(contexto, mapper, mock.Object);

            var respuesta = await controller.Post(actor);
            var resultado = respuesta as CreatedAtRouteResult;

            Assert.AreEqual(201, resultado.StatusCode);

            var contexto2 = ConstruirContexto(nombreDb);
            var listado = await contexto2.Actores.ToListAsync();

            Assert.AreEqual(1, listado.Count);
            Assert.IsNull(listado[0].Foto);

            Assert.AreEqual(0, mock.Invocations.Count); // Para chequear que no se llame el guardar archivos
        }

        [TestMethod]
        public async Task CrearActorConFoto()
        {
            var nombreDb = Guid.NewGuid().ToString();
            var contexto = ConstruirContexto(nombreDb);
            var mapper = ConfigurarAutoMapper();

            var contenido = Encoding.UTF8.GetBytes("Imagen de prueba");
            var archivo = new FormFile(new MemoryStream(contenido), 0, contenido.Length, "Data", "imagen.jpg");
            archivo.Headers = new HeaderDictionary();
            archivo.ContentType = "image/jpg";

            var actor = new ActorCreacionDto()
            {
                Nombre = "nuevo actor",
                FechaNacimiento = DateTime.Now,
                Foto = archivo
            };

            var mock = new Mock<IAlmacenadorArchivos>();
            mock.Setup(x => x.GuardarArchivo(contenido, ".jpg", "Actores", archivo.ContentType))
                .Returns(Task.FromResult("url"));

            var controller = new ActoresController(contexto, mapper, mock.Object);
            var respuesta = await controller.Post(actor);
            var resultado = respuesta as CreatedAtRouteResult;

            Assert.AreEqual(201, resultado.StatusCode);

            var contexto2 = ConstruirContexto(nombreDb);
            var listado = await contexto2.Actores.ToListAsync();

            Assert.AreEqual(1, listado.Count);
            Assert.AreEqual("url", listado[0].Foto);
            Assert.AreEqual(1, mock.Invocations.Count);
        }

        [TestMethod]
        public async Task PatchRetorna404SiActorNoExiste()
        {
            var nombreDb = Guid.NewGuid().ToString();
            var contexto = ConstruirContexto(nombreDb);
            var mapper = ConfigurarAutoMapper();

            var controller = new ActoresController(contexto, mapper, null);

            var patchDoc = new JsonPatchDocument<ActorPatchDto>();
            var respuesta = await controller.Patch(1, patchDoc);
            var resultado = respuesta as StatusCodeResult;

            Assert.AreEqual(404, resultado.StatusCode);
        }

        [TestMethod]
        public async Task PatchActualizaUnSoloCampo()
        {
            var nombreDb = Guid.NewGuid().ToString();
            var contexto = ConstruirContexto(nombreDb);
            var mapper = ConfigurarAutoMapper();

            var fechaNacimiento = DateTime.Now;
            var actor = new Actor() { Nombre = "Pablo", FechaNacimiento = fechaNacimiento };

            contexto.Add(actor);

            await contexto.SaveChangesAsync();

            var contexto2 = ConstruirContexto(nombreDb);

            var controller = new ActoresController(contexto2, mapper, null);

            var objectValidator = new Mock<IObjectModelValidator>();

            objectValidator.Setup(x => x.Validate(It.IsAny<ActionContext>(),
                It.IsAny<ValidationStateDictionary>(),
                It.IsAny<string>(),
                It.IsAny<object>()));

            controller.ObjectValidator = objectValidator.Object;

            var patchDoc = new JsonPatchDocument<ActorPatchDto>();

            patchDoc.Operations.Add(new Operation<ActorPatchDto>("replace", "/nombre", null, "Jose"));

            var respuesta = await controller.Patch(1, patchDoc);
            var resultado = respuesta as StatusCodeResult;

            Assert.AreEqual(204, resultado.StatusCode);

            var contexto3 = ConstruirContexto(nombreDb);
            var actorDB = await contexto3.Actores.FirstAsync();

            Assert.AreEqual("Jose", actorDB.Nombre);
            Assert.AreEqual(fechaNacimiento, actorDB.FechaNacimiento);
        }
    }
}
