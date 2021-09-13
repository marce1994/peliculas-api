using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeliculasApi.DTOs;
using PeliculasApi.Entidades;
using PeliculasApi.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PeliculasApi.Controllers
{
    [Route("api/peliculas/{peliculaId:int}/[controller]")]
    [ServiceFilter(typeof(PeliculaExisteAttribute))]
    public class ReviewsController : CustomBaseController
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public ReviewsController(ApplicationDbContext context, IMapper mapper) : base(context, mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<ReviewDto>>> Get(int peliculaId, [FromQuery] PaginacionDto paginacionDto)
        {
            var queryable = context.Reviews.Include(x => x.Usuario).AsQueryable();
            queryable = queryable.Where(x => x.PeliculaId == peliculaId);
            return await Get<Review, ReviewDto>(paginacionDto, queryable);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Post(int peliculaId, [FromBody] ReviewCreacionDto reviewCreacionDto)
        {
            var usuarioId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value; // No ponerlo en el Dto porque es una mala practica.. es inseguro..

            var reviewExiste = await context.Reviews
                .AnyAsync(x => x.PeliculaId == peliculaId && x.UsuarioId == usuarioId);

            if (reviewExiste)
                return BadRequest("El usuario ya ha escrito un review de esta pelicula");

            var review = mapper.Map<Review>(reviewCreacionDto);
            review.PeliculaId = peliculaId;
            review.UsuarioId = usuarioId;

            context.Add(review);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{reviewId:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Put(int peliculaId, int reviewId, [FromBody] ReviewCreacionDto reviewCreacionDto)
        {
            var reviewDb = await context.Reviews.FirstOrDefaultAsync(x => x.Id == reviewId);

            if (reviewDb == null)
                return NotFound();

            var usuarioId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            
            if (reviewDb.UsuarioId != usuarioId)
                return BadRequest("No tiene permisos de editar este review");

            reviewDb = mapper.Map(reviewCreacionDto, reviewDb);

            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{reviewId:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Delete(int reviewId)
        {
            var reviewDb = await context.Reviews.FirstOrDefaultAsync(x => x.Id == reviewId);

            if (reviewDb == null)
                return NotFound();

            var usuarioId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;

            if (reviewDb.UsuarioId != usuarioId)
                return BadRequest("No tiene permisos de editar este review");

            context.Remove(reviewDb);
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}
