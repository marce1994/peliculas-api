using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeliculasApi.DTOs;
using PeliculasApi.Entidades;
using PeliculasApi.Helpers;
using PeliculasApi.Servicios;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using Microsoft.Extensions.Logging;

namespace PeliculasApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PeliculasController : CustomBaseController
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IAlmacenadorArchivos almacenadorArchivos;
        private readonly ILogger<PeliculasController> logger;
        private readonly string contenedor = "Peliculas";

        public PeliculasController(ApplicationDbContext context, IMapper mapper, IAlmacenadorArchivos almacenadorArchivos, ILogger<PeliculasController> logger) : base(context, mapper)
        {
            this.context = context;
            this.mapper = mapper;
            this.almacenadorArchivos = almacenadorArchivos;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<PeliculasIndexDto>> Get()
        {
            var top = 5;
            var hoy = DateTime.Today;

            var proximosEstrenos = await context.Peliculas
                .Where(x => x.FechaEstreno > hoy)
                .OrderBy(x => x.FechaEstreno)
                .Take(top)
                .ToListAsync();

            var enCines = await context.Peliculas
                .Where(x => x.EnCines)
                .OrderBy(x => x.FechaEstreno)
                .Take(top)
                .ToListAsync();

            var resultado = new PeliculasIndexDto()
            {
                FuturosEstrenos = mapper.Map<List<PeliculaDto>>(proximosEstrenos),
                EnCines = mapper.Map<List<PeliculaDto>>(enCines)
            };

            return resultado;
        }

        [HttpGet("filtro")]
        public async Task<ActionResult<List<PeliculaDto>>> Filtrar([FromQuery] FiltroPeliculasDto filtroPeliculasDto)
        {
            var queryable = context.Peliculas.AsQueryable();

            if (!string.IsNullOrEmpty(filtroPeliculasDto.Titulo))
                queryable = queryable.Where(x => x.Titulo.Contains(filtroPeliculasDto.Titulo));

            if(filtroPeliculasDto.EnCines)
                queryable = queryable.Where(x => x.EnCines);

            if (filtroPeliculasDto.ProximosEstrenos)
                queryable = queryable.Where(x => x.FechaEstreno > DateTime.Today);

            if(filtroPeliculasDto.GeneroId != 0)
                queryable = queryable.Where(x => x.PeliculasGeneros.Select(y => y.GeneroId).Contains(filtroPeliculasDto.GeneroId));

            if (!string.IsNullOrEmpty(filtroPeliculasDto.CampoOrdenar))
            {
                var tipoOrden = filtroPeliculasDto.OrdenAscendente ? "ascending" : "descending";
                try
                {
                    queryable = queryable.OrderBy($"{filtroPeliculasDto.CampoOrdenar} {tipoOrden}");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message, ex);
                }
            }

            await HttpContext.InsertarParametrosPaginacion(queryable, filtroPeliculasDto.CantidadRegistrosPorPagina);

            var entidades = await queryable.Paginar(filtroPeliculasDto.Paginacion).ToListAsync();

            return mapper.Map<List<PeliculaDto>>(entidades);
        }

        [HttpGet("{id:int}", Name = "obtenerPelicula")]
        public async Task<ActionResult<PeliculaDetallesDto>> Get(int id)
        {
            var entidad = await context.Peliculas
                .Include(x => x.PeliculasActores)
                .ThenInclude(x => x.Actor)
                .Include(x => x.PeliculasGeneros)
                .ThenInclude(x => x.Genero)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entidad == null)
                return NotFound();

            entidad.PeliculasActores = entidad.PeliculasActores.OrderBy(x => x.Orden).ToList();

            return entidad == null ? NotFound() : mapper.Map<PeliculaDetallesDto>(entidad);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromForm] PeliculaCreacionDto peliculaCreacionDto)
        {
            var entidad = mapper.Map<Pelicula>(peliculaCreacionDto);

            if (peliculaCreacionDto.Poster != null)
                using (var memoryStream = new MemoryStream())
                {
                    await peliculaCreacionDto.Poster.CopyToAsync(memoryStream);
                    var contenido = memoryStream.ToArray();
                    var extension = Path.GetExtension(peliculaCreacionDto.Poster.FileName);
                    entidad.Poster = await almacenadorArchivos.GuardarArchivo(contenido, extension, contenedor, peliculaCreacionDto.Poster.ContentType);
                }

            AsignarOrdenActores(entidad);

            context.Peliculas.Add(entidad);

            await context.SaveChangesAsync();

            var peliculaDto = mapper.Map<PeliculaDto>(entidad);

            return CreatedAtRoute("obtenerPelicula", new { id = peliculaDto.Id }, peliculaDto);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, [FromForm] PeliculaCreacionDto peliculaCreacionDto)
        {
            var entidad = await context.Peliculas
                .Include(x => x.PeliculasActores)
                .Include(x => x.PeliculasGeneros)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entidad == null)
                return NotFound();

            entidad = mapper.Map(peliculaCreacionDto, entidad);

            if (peliculaCreacionDto.Poster != null)
                using (var memoryStream = new MemoryStream())
                {
                    await peliculaCreacionDto.Poster.CopyToAsync(memoryStream);
                    var contenido = memoryStream.ToArray();
                    var extension = Path.GetExtension(peliculaCreacionDto.Poster.FileName);
                    entidad.Poster = await almacenadorArchivos.EditarArchivo(contenido, extension, contenedor, entidad.Poster, peliculaCreacionDto.Poster.ContentType);
                }

            AsignarOrdenActores(entidad);

            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await context.Peliculas.AnyAsync(x => x.Id == id);

            if (!existe)
                return NotFound();

            var entidad = await context.Peliculas.FirstOrDefaultAsync(x => x.Id == id);
            var poster = entidad.Poster;

            context.Remove(entidad);

            await context.SaveChangesAsync();

            await almacenadorArchivos.BorrarArchivo(poster, contenedor);

            return NoContent();
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> Patch(int id, [FromBody] JsonPatchDocument<PeliculaPatchDto> patchDocument)
        {
            return await Patch<Pelicula, PeliculaPatchDto>(id, patchDocument);
        }

        private void AsignarOrdenActores(Pelicula pelicula)
        {
            if (pelicula.PeliculasActores != null)
                for (int i = 0; i < pelicula.PeliculasActores.Count; i++)
                {
                    pelicula.PeliculasActores[i].Orden = i;
                }
        }
    }
}
