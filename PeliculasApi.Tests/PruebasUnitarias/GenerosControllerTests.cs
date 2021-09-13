using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeliculasApi.Controllers;
using PeliculasApi.DTOs;
using PeliculasApi.Entidades;
using System;
using System.Threading.Tasks;

namespace PeliculasApi.Tests.PruebasUnitarias
{
    [TestClass]
    public class GenerosControllerTests : BasePruebas
    {
        [TestMethod]
        public async Task ObtenerTodosLosGeneros()
        {
            var nombreDb = Guid.NewGuid().ToString();

            var contexto = ConstruirContexto(nombreDb);
            var mapper = ConfigurarAutoMapper();

            contexto.Generos.Add(new Genero { Nombre = "Genero1" });
            contexto.Generos.Add(new Genero { Nombre = "Genero2" });
            await contexto.SaveChangesAsync();

            var contexto2 = ConstruirContexto(nombreDb); // Para evitar que ef los traiga de memoria...

            var target = new GenerosController(contexto2, mapper);

            var respuesta = await target.Get();

            var generos = respuesta.Value;

            Assert.AreEqual(2, generos.Count);
        }

        [TestMethod]
        public async Task ObtenerGeneroPorIdNoExistente()
        {
            var nombreDb = Guid.NewGuid().ToString();

            var contexto = ConstruirContexto(nombreDb);
            var mapper = ConfigurarAutoMapper();

            var target = new GenerosController(contexto, mapper);

            var respuesta = await target.Get(1);

            var genero = respuesta.Result as StatusCodeResult;

            Assert.AreEqual(404, genero.StatusCode);
        }

        [TestMethod]
        public async Task ObtenerGeneroPorIdExistente()
        {
            var nombreDb = Guid.NewGuid().ToString();

            var contexto = ConstruirContexto(nombreDb);
            var mapper = ConfigurarAutoMapper();

            contexto.Generos.Add(new Genero { Nombre = "Genero1" });
            contexto.Generos.Add(new Genero { Nombre = "Genero2" });
            await contexto.SaveChangesAsync();

            var contexto2 = ConstruirContexto(nombreDb); // Para evitar que ef los traiga de memoria...

            var target = new GenerosController(contexto2, mapper);

            var id = 1;
            var respuesta = await target.Get(id);

            var genero = respuesta.Value;

            Assert.AreEqual(id, genero.Id);
        }

        [TestMethod]
        public async Task CrearGenero()
        {
            var nombreDb = Guid.NewGuid().ToString();

            var contexto = ConstruirContexto(nombreDb);
            var mapper = ConfigurarAutoMapper();

            var nuevoGenero = new GeneroCreacionDto()
            {
                Nombre = "NuevoGenero"
            };

            var target = new GenerosController(contexto, mapper);

            var respuesta = await target.Post(nuevoGenero);
            var resultado = respuesta as CreatedAtRouteResult;

            Assert.IsNotNull(resultado);

            var contexto2 = ConstruirContexto(nombreDb); // Para evitar que ef los traiga de memoria...
            var cantidad = await contexto2.Generos.CountAsync();

            Assert.AreEqual(1, cantidad);
        }

        [TestMethod]
        public async Task ActualizarGenero()
        {
            var nombreDb = Guid.NewGuid().ToString();

            var contexto = ConstruirContexto(nombreDb);
            var mapper = ConfigurarAutoMapper();

            contexto.Generos.Add(new Genero { Nombre = "Genero1" });
            await contexto.SaveChangesAsync();

            var contexto2 = ConstruirContexto(nombreDb); // Para evitar que ef los traiga de memoria...

            var target = new GenerosController(contexto2, mapper);

            var generoCreacionDto = new GeneroCreacionDto { Nombre = "Nuevo Nombre" };
            var id = 1;

            var respuesta = await target.Put(id, generoCreacionDto);
            var resultado = respuesta as StatusCodeResult;

            Assert.AreEqual(204, resultado.StatusCode);

            var contexto3 = ConstruirContexto(nombreDb); // Para evitar que ef los traiga de memoria...
            var existe = await contexto3.Generos.AnyAsync();

            Assert.IsTrue(existe);
        }


        [TestMethod]
        public async Task IntentarBorrarGeneroNoExistente()
        {
            var nombreDb = Guid.NewGuid().ToString();

            var contexto = ConstruirContexto(nombreDb);
            var mapper = ConfigurarAutoMapper();

            var target = new GenerosController(contexto, mapper);

            var respuesta = await target.Delete(1);
            var resultado = respuesta as StatusCodeResult;

            Assert.AreEqual(404, resultado.StatusCode);
        }

        [TestMethod]
        public async Task BorrarGenero()
        {
            var nombreDb = Guid.NewGuid().ToString();

            var contexto = ConstruirContexto(nombreDb);
            var mapper = ConfigurarAutoMapper();

            contexto.Generos.Add(new Genero { Nombre = "Genero1" });
            await contexto.SaveChangesAsync();

            var contexto2 = ConstruirContexto(nombreDb); // Para evitar que ef los traiga de memoria...

            var target = new GenerosController(contexto2, mapper);

            var respuesta = await target.Delete(1);
            var resultado = respuesta as StatusCodeResult;

            Assert.AreEqual(204, resultado.StatusCode);

            var contexto3 = ConstruirContexto(nombreDb); // Para evitar que ef los traiga de memoria...
            var existe = await contexto3.Generos.AnyAsync();

            Assert.IsFalse(existe);
        }
    }
}
