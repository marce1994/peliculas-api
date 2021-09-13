using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using PeliculasApi.DTOs;
using PeliculasApi.Entidades;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeliculasApi.Tests.PruebasIntegracion
{
    [TestClass]
    public class ReviewsControllerTests : BasePruebas
    {
        private static readonly string url = "/api/peliculas/1/reviews";

        [TestMethod]
        public async Task ObtenerReviewsDevuelve404PeliculaInexistente()
        {
            var nombreDb = Guid.NewGuid().ToString();
            var factory = ConstruirWebApplicationFactory(nombreDb);

            var cliente = factory.CreateClient();
            var respuesta = await cliente.GetAsync(url);

            Assert.AreEqual(404, (int)respuesta.StatusCode);
        }

        [TestMethod]
        public async Task ObtenerReviewsDevuelveListadoVacio()
        {
            var nombreDb = Guid.NewGuid().ToString();
            var factory = ConstruirWebApplicationFactory(nombreDb);
            var context = ConstruirContexto(nombreDb);

            context.Peliculas.Add(new Pelicula() { Titulo = "Película 1" });
            await context.SaveChangesAsync();

            var cliente = factory.CreateClient();
            var respuesta = await cliente.GetAsync(url);

            respuesta.EnsureSuccessStatusCode();

            var reviews = JsonConvert.DeserializeObject<List<ReviewDto>>(await respuesta.Content.ReadAsStringAsync());

            Assert.AreEqual(0, reviews.Count);
        }
    }
}
