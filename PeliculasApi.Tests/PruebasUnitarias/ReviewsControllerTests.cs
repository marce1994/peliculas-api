using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PeliculasApi.Controllers;
using PeliculasApi.DTOs;
using PeliculasApi.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeliculasApi.Tests.PruebasUnitarias
{
    [TestClass]
    public class ReviewsControllerTests : BasePruebas
    {
        [TestMethod]
        public async Task UsuarioNoPuedeCrearDosReviewsParaLaMismaPelicula()
        {
            var nombreDb = Guid.NewGuid().ToString();
            var contexto = ConstruirContexto(nombreDb);
            CrearPeliculas(nombreDb);

            var peliculaId = contexto.Peliculas.Select(x => x.Id).First();

            var review1 = new Review()
            {
                PeliculaId = peliculaId,
                UsuarioId = usuarioPorDefectoId,
                Puntuacion = 5
            };

            contexto.Add(review1);
            await contexto.SaveChangesAsync();

            var contexto2 = ConstruirContexto(nombreDb);
            var mapper = ConfigurarAutoMapper();

            var controller = new ReviewsController(contexto2, mapper);
            controller.ControllerContext = ConstruirControllerContext();

            var reviewCreacionDto = new ReviewCreacionDto { Puntuacion = 5 };
            var respuesta = await controller.Post(peliculaId, reviewCreacionDto);

            var valor = respuesta as IStatusCodeActionResult;

            Assert.AreEqual(400, valor.StatusCode.Value);
        }

        [TestMethod]
        public async Task CrearReview()
        {
            var nombreDb = Guid.NewGuid().ToString();
            var contexto = ConstruirContexto(nombreDb);
            CrearPeliculas(nombreDb);

            var peliculaId = contexto.Peliculas.Select(x => x.Id).First();
            var contexto2 = ConstruirContexto(nombreDb);

            var mapper = ConfigurarAutoMapper();
            var controller = new ReviewsController(contexto2, mapper);
            controller.ControllerContext = ConstruirControllerContext();

            var reviewCreacionDto = new ReviewCreacionDto() { Puntuacion = 5 };
            var respuesta = await controller.Post(peliculaId, reviewCreacionDto);

            var valor = respuesta as NoContentResult;

            Assert.IsNotNull(valor);

            var contexto3 = ConstruirContexto(nombreDb);
            var reviewDb = contexto3.Reviews.First();

            Assert.AreEqual(usuarioPorDefectoId, reviewDb.UsuarioId);
        }

        private void CrearPeliculas(string nombreDB)
        {
            var contexto = ConstruirContexto(nombreDB);

            contexto.Peliculas.Add(new Pelicula() { Titulo = "pelicula 1" });

            contexto.SaveChanges();
        }
    }
}
