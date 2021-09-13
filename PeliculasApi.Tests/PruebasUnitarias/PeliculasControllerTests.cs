using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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
    public class PeliculasControllerTests : BasePruebas
    {
        private string CrearDataPrueba()
        {
            var nombreDb = Guid.NewGuid().ToString();
            var context = ConstruirContexto(nombreDb);
            var genero = new Genero() { Nombre = "Genero 1" };

            var peliculas = new List<Pelicula>()
            {
                new Pelicula(){Titulo = "Pelicula 1", FechaEstreno = new DateTime(2010, 1,1), EnCines = false},
                new Pelicula(){Titulo = "No estrenada", FechaEstreno = DateTime.Today.AddDays(1), EnCines = false},
                new Pelicula(){Titulo = "Pelicula en cines", FechaEstreno = DateTime.Today.AddDays(-1), EnCines = true}
            };

            var peliculaConGenero = new Pelicula()
            {
                Titulo = "Pelicula con genero",
                FechaEstreno = new DateTime(2010, 1, 1),
                EnCines = false
            };

            peliculas.Add(peliculaConGenero);

            context.Add(genero);
            context.AddRange(peliculas);

            context.SaveChanges();

            var peliculaGenero = new PeliculasGeneros() { GeneroId = genero.Id, PeliculaId = peliculaConGenero.Id };

            context.Add(peliculaGenero);
            context.SaveChanges();

            return nombreDb;
        }

        [TestMethod]
        public async Task FiltrarPorTitulo()
        {
            var nombreDb = CrearDataPrueba();
            var mapper = ConfigurarAutoMapper();
            var contexto = ConstruirContexto(nombreDb);

            var controller = new PeliculasController(contexto, mapper, null, null);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var tituloPelicula = "Pelicula 1";

            var filtroDTO = new FiltroPeliculasDto()
            {
                Titulo = tituloPelicula,
                CantidadRegistrosPorPagina = 10
            };

            var respuesta = await controller.Filtrar(filtroDTO);
            var peliculas = respuesta.Value;

            Assert.AreEqual(1, peliculas.Count);
            Assert.AreEqual(tituloPelicula, peliculas[0].Titulo);
        }

        [TestMethod]
        public async Task FiltrarEnCines()
        {
            var nombreDb = CrearDataPrueba();
            var mapper = ConfigurarAutoMapper();
            var contexto = ConstruirContexto(nombreDb);

            var controller = new PeliculasController(contexto, mapper, null, null);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var filtroDto = new FiltroPeliculasDto()
            {
                EnCines = true
            };

            var respuesta = await controller.Filtrar(filtroDto);
            var peliculas = respuesta.Value;

            Assert.AreEqual(1, peliculas.Count);
            Assert.AreEqual("Pelicula en cines", peliculas[0].Titulo);
        }

        [TestMethod]
        public async Task FiltrarProximosEstrenos()
        {
            var nombreDb = CrearDataPrueba();
            var mapper = ConfigurarAutoMapper();
            var contexto = ConstruirContexto(nombreDb);

            var controller = new PeliculasController(contexto, mapper, null, null);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var filtroDto = new FiltroPeliculasDto()
            {
                ProximosEstrenos = true
            };

            var respuesta = await controller.Filtrar(filtroDto);
            var peliculas = respuesta.Value;

            Assert.AreEqual(1, peliculas.Count);
            Assert.AreEqual("No estrenada", peliculas[0].Titulo);
        }

        [TestMethod]
        public async Task FiltrarPorGenero()
        {
            var nombreDb = CrearDataPrueba();
            var mapper = ConfigurarAutoMapper();
            var contexto = ConstruirContexto(nombreDb);

            var controller = new PeliculasController(contexto, mapper, null, null);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var generoId = contexto.Generos.Select(x => x.Id).First();

            var filtroDto = new FiltroPeliculasDto()
            {
                GeneroId = generoId
            };

            var respuesta = await controller.Filtrar(filtroDto);
            var peliculas = respuesta.Value;

            Assert.AreEqual(1, peliculas.Count);
            Assert.AreEqual("Pelicula con genero", peliculas[0].Titulo);
        }

        [TestMethod]
        public async Task FiltrarOrdenaTituloAscendente()
        {
            var nombreDb = CrearDataPrueba();
            var mapper = ConfigurarAutoMapper();
            var contexto = ConstruirContexto(nombreDb);

            var controller = new PeliculasController(contexto, mapper, null, null);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var filtroDto = new FiltroPeliculasDto()
            {
                CampoOrdenar = "titulo",
                OrdenAscendente = true
            };

            var respuesta = await controller.Filtrar(filtroDto);
            var peliculas = respuesta.Value;

            var contexto2 = ConstruirContexto(nombreDb);
            var peliculasDb = contexto2.Peliculas.OrderBy(x => x.Titulo).ToList();

            Assert.AreEqual(peliculasDb.Count, peliculas.Count);

            for (int i = 0; i < peliculasDb.Count; i++)
            {
                var peliculaControlador = peliculas[i];
                var peliculaDb = peliculasDb[i];

                Assert.AreEqual(peliculaDb.Id, peliculaControlador.Id);
            }
        }

        [TestMethod]
        public async Task FiltrarTituloDescendente()
        {
            var nombreDb = CrearDataPrueba();
            var mapper = ConfigurarAutoMapper();
            var contexto = ConstruirContexto(nombreDb);

            var controller = new PeliculasController(contexto, mapper, null, null);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var filtroDTO = new FiltroPeliculasDto()
            {
                CampoOrdenar = "titulo",
                OrdenAscendente = false
            };

            var respuesta = await controller.Filtrar(filtroDTO);
            var peliculas = respuesta.Value;

            var contexto2 = ConstruirContexto(nombreDb);
            var peliculasDB = contexto2.Peliculas.OrderByDescending(x => x.Titulo).ToList();

            Assert.AreEqual(peliculasDB.Count, peliculas.Count);

            for (int i = 0; i < peliculasDB.Count; i++)
            {
                var peliculaDelControlador = peliculas[i];
                var peliculaDB = peliculasDB[i];

                Assert.AreEqual(peliculaDB.Id, peliculaDelControlador.Id);
            }
        }

        [TestMethod]
        public async Task FiltrarPorCampoIncorrectoDevuelvePeliculas()
        {
            var nombreDb = CrearDataPrueba();
            var mapper = ConfigurarAutoMapper();
            var contexto = ConstruirContexto(nombreDb);

            var mock = new Mock<ILogger<PeliculasController>>();

            var controller = new PeliculasController(contexto, mapper, null, mock.Object);
            controller.ControllerContext.HttpContext = new DefaultHttpContext();

            var filtroDto = new FiltroPeliculasDto()
            {
                CampoOrdenar = "sarasa",
                OrdenAscendente = true
            };

            var respuesta = await controller.Filtrar(filtroDto);
            var peliculas = respuesta.Value;

            var contexto2 = ConstruirContexto(nombreDb);
            var peliculasDb = contexto2.Peliculas.ToList();

            Assert.AreEqual(peliculasDb.Count, peliculas.Count);
            Assert.AreEqual(1, mock.Invocations.Count);
        }
    }
}
