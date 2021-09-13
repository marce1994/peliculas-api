using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace PeliculasApi.Helpers
{
    public class PeliculaExisteAttribute : Attribute, IAsyncResourceFilter
    {
        private readonly ApplicationDbContext dbContext;

        public PeliculaExisteAttribute(ApplicationDbContext context)
        {
            this.dbContext = context;
        }

        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            var peliculaIdObject = context.HttpContext.Request.RouteValues["peliculaId"];

            if (peliculaIdObject == null)
                return;

            var peliculaId = int.Parse(peliculaIdObject.ToString());

            var existePelicula = await dbContext.Peliculas.AnyAsync(x => x.Id == peliculaId);

            if (!existePelicula)
                context.Result = new NotFoundResult();
            else
                await next();
        }
    }
}
