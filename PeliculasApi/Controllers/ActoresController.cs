using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeliculasApi.DTOs;
using PeliculasApi.Entidades;
using PeliculasApi.Servicios;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PeliculasApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActoresController : CustomBaseController
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IAlmacenadorArchivos almacenadorArchivos;
        private readonly string contenedor = "Actores";

        public ActoresController(ApplicationDbContext context, IMapper mapper, IAlmacenadorArchivos almacenadorArchivos) : base (context, mapper)
        {
            this.context = context;
            this.mapper = mapper;
            this.almacenadorArchivos = almacenadorArchivos;
        }

        [HttpGet]
        public async Task<ActionResult<List<ActorDto>>> Get([FromQuery] PaginacionDto paginacionDto)
        {
            return await Get<Actor, ActorDto>(paginacionDto);
        }

        [HttpGet("{id:int}", Name = "obtenerActor")]
        public async Task<ActionResult<ActorDto>> Get(int id)
        {
            return await Get<Actor, ActorDto>(id);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromForm] ActorCreacionDto actorCreacionDto)
        {
            var entidad = mapper.Map<Actor>(actorCreacionDto);

            if (actorCreacionDto.Foto != null)
                using (var memoryStream = new MemoryStream())
                {
                    await actorCreacionDto.Foto.CopyToAsync(memoryStream);
                    var contenido = memoryStream.ToArray();
                    var extension = Path.GetExtension(actorCreacionDto.Foto.FileName);
                    entidad.Foto = await almacenadorArchivos.GuardarArchivo(contenido, extension, contenedor, actorCreacionDto.Foto.ContentType);
                }

            context.Actores.Add(entidad);

            await context.SaveChangesAsync();

            var actorDto = mapper.Map<ActorDto>(entidad);

            return CreatedAtRoute("obtenerActor", new { id = actorDto.Id }, actorDto);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, [FromForm] ActorCreacionDto actorCreacionDto)
        {
            var actorDb = await context.Actores.FirstOrDefaultAsync(x => x.Id == id);

            if (actorDb == null)
                return NotFound();

            actorDb = mapper.Map(actorCreacionDto, actorDb);

            if (actorCreacionDto.Foto != null)
                using (var memoryStream = new MemoryStream())
                {
                    await actorCreacionDto.Foto.CopyToAsync(memoryStream);
                    var contenido = memoryStream.ToArray();
                    var extension = Path.GetExtension(actorCreacionDto.Foto.FileName);
                    actorDb.Foto = await almacenadorArchivos.EditarArchivo(contenido, extension, contenedor, actorDb.Foto, actorCreacionDto.Foto.ContentType);
                }

            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await context.Actores.AnyAsync(x => x.Id == id);

            if (!existe)
                return NotFound();

            var actor = await context.Actores.FirstOrDefaultAsync(x => x.Id == id);
            var foto = actor.Foto;

            context.Remove(actor);

            await context.SaveChangesAsync();

            await almacenadorArchivos.BorrarArchivo(foto, contenedor);

            return NoContent();
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> Patch(int id, [FromBody] JsonPatchDocument<ActorPatchDto> patchDocument)
        {
            return await Patch<Actor, ActorPatchDto>(id, patchDocument);
        }
    }
}
