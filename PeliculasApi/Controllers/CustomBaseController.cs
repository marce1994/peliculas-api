using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeliculasApi.DTOs;
using PeliculasApi.Entidades;
using PeliculasApi.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeliculasApi.Controllers
{
    public class CustomBaseController: ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public CustomBaseController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        protected async Task<List<TDTO>> Get<TEntidad, TDTO>()
            where TEntidad : class
        {
            var entidades = await context.Set<TEntidad>()
                .AsNoTracking()
                .ToListAsync();

            var dtos = mapper.Map<List<TDTO>>(entidades);
            return dtos;
        }

        protected async Task<List<TDTO>> Get<TEntidad, TDTO>(PaginacionDto paginacionDto)
            where TEntidad : class
        {
            var queryable = context.Set<TEntidad>().AsQueryable();
            return await Get<TEntidad, TDTO>(paginacionDto, queryable);
        }

        protected async Task<List<TDTO>> Get<TEntidad, TDTO>(PaginacionDto paginacionDTO, IQueryable<TEntidad> queryable)
            where TEntidad : class
        {
            await HttpContext.InsertarParametrosPaginacion(queryable, paginacionDTO.CantidadRegistrosPorPagina);
            var entidades = await queryable.Paginar(paginacionDTO).ToListAsync();
            return mapper.Map<List<TDTO>>(entidades);
        }

        protected async Task<ActionResult<TDTO>> Get<TEntidad, TDTO>(int id)
            where TEntidad : class, IId
        {
            var entidad = await context.Set<TEntidad>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entidad == null)
                return NotFound();

            var dto = mapper.Map<TDTO>(entidad);
            return dto;
        }

        protected async Task<ActionResult> Post<TCreacion, TEntidad, TLectura>(TCreacion creacionDto, string nombreRuta)
            where TEntidad : class, IId
        {
            var entidad = mapper.Map<TEntidad>(creacionDto);
            context.Add(entidad);
            await context.SaveChangesAsync();
            var dtoLectura = mapper.Map<TLectura>(entidad);
            return new CreatedAtRouteResult(nombreRuta, new { id = entidad.Id }, dtoLectura);
        }

        protected async Task<ActionResult> Put<TCreacion, TEntidad>(int id, TCreacion creacionDto)
            where TEntidad : class, IId
        {
            var entidad = mapper.Map<TEntidad>(creacionDto);
            entidad.Id = id;
            context.Entry(entidad).State = EntityState.Modified;
            await context.SaveChangesAsync();
            return NoContent();
        }

        protected async Task<ActionResult> Delete<TEntidad>(int id)
            where TEntidad : class, IId, new()
        {
            var existe = await context.Set<TEntidad>().AnyAsync(x => x.Id == id);

            if (!existe)
                return NotFound();

            context.Remove(new TEntidad() { Id = id });

            await context.SaveChangesAsync();

            return NoContent();
        }

        protected async Task<ActionResult> Patch<TEntidad, TDTO>(int id, JsonPatchDocument<TDTO> patchDocument)
            where TDTO : class
            where TEntidad : class, IId
        {
            if (patchDocument == null)
                return BadRequest();

            var entidad = await context.Set<TEntidad>().FirstOrDefaultAsync(x => x.Id == id);

            if (entidad == null)
                return NotFound();

            var dto = mapper.Map<TDTO>(entidad);

            patchDocument.ApplyTo(dto, ModelState);

            var esValido = TryValidateModel(dto);

            if (!esValido)
                return BadRequest(ModelState);

            mapper.Map(dto, entidad);

            await context.SaveChangesAsync();

            return NoContent();
        }
    }
}
