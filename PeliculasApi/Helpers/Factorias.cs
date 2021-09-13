using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PeliculasApi.Servicios;
using System;

namespace PeliculasApi.Helpers
{
    public static class Factorias
    {
        public static IAlmacenadorArchivos AlmacenadorArchivosService(IServiceProvider serviceProvider)
        {
            var env = serviceProvider.GetRequiredService<IWebHostEnvironment>();
            if (env.IsDevelopment())
            {
                var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
                return new AlmacenadorArchivosLocal(env, httpContextAccessor);
            }
            else
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                return new AlmacenadorArchivosAzure(configuration);
            }
        }
    }
}
