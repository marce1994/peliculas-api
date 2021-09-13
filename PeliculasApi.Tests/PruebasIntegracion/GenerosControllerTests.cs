using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using PeliculasApi.DTOs;
using PeliculasApi.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeliculasApi.Tests.PruebasIntegracion
{
    [TestClass]
    public class GenerosControllerTests : BasePruebas
    {
        private static readonly string url = "/api/generos";

        [TestMethod]
        public async Task ObtenerTodosLosGenerosListadoVacio()
        {
            var nombreDb = Guid.NewGuid().ToString();
            var factory = ConstruirWebApplicationFactory(nombreDb);

            var cliente = factory.CreateClient();
            var respuesta = await cliente.GetAsync(url);

            respuesta.EnsureSuccessStatusCode();

            var generos = JsonConvert.DeserializeObject<List<GeneroDto>>(await respuesta.Content.ReadAsStringAsync());

            Assert.AreEqual(0, generos.Count);
        }

        [TestMethod]
        public async Task ObtenerTodosLosGeneros()
        {
            var nombreDb = Guid.NewGuid().ToString();
            var factory = ConstruirWebApplicationFactory(nombreDb);

            var contexto = ConstruirContexto(nombreDb);

            contexto.Generos.Add(new Genero() { Nombre = "Género 1" });
            contexto.Generos.Add(new Genero() { Nombre = "Género 2" });

            await contexto.SaveChangesAsync();

            var cliente = factory.CreateClient();
            var respuesta = await cliente.GetAsync(url);

            respuesta.EnsureSuccessStatusCode();

            var generos = JsonConvert.DeserializeObject<List<GeneroDto>>(await respuesta.Content.ReadAsStringAsync());

            Assert.AreEqual(2, generos.Count);
        }

        [TestMethod]
        public async Task BorrarGenero()
        {
            var nombreDb = Guid.NewGuid().ToString();
            var factory = ConstruirWebApplicationFactory(nombreDb);

            var contexto = ConstruirContexto(nombreDb);

            contexto.Generos.Add(new Genero() { Nombre = "Género 1" });

            await contexto.SaveChangesAsync();

            var cliente = factory.CreateClient();
            var respuesta = await cliente.DeleteAsync($"{url}/1");

            respuesta.EnsureSuccessStatusCode();

            var contexto2 = ConstruirContexto(nombreDb);
            var existe = await contexto2.Generos.AnyAsync();

            Assert.IsFalse(existe);
        }

        [TestMethod]
        public async Task BorrarGeneroRetorna401()
        {
            var nombreDb = Guid.NewGuid().ToString();
            var factory = ConstruirWebApplicationFactory(nombreDb, ignorarSeguridad: false);

            var cliente = factory.CreateClient();
            var respuesta = await cliente.DeleteAsync($"{url}/1");

            Assert.AreEqual("Unauthorized", respuesta.ReasonPhrase);
        }
    }
}
