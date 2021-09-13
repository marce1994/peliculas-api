using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite;
using PeliculasApi.Helpers;
using System.Linq;
using System.Security.Claims;

namespace PeliculasApi.Tests
{
    public class BasePruebas
    {
        protected string usuarioPorDefectoId = "31bf5a3d-b126-49e3-8705-3319471e614f";
        protected string usuarioPorDefectoEmail = "ejemplo@hotmail.com";

        protected ApplicationDbContext ConstruirContexto(string nombreDb)
        {
            var opciones = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(nombreDb).Options;

            var dbContext = new ApplicationDbContext(opciones);
            return dbContext;
        }

        protected IMapper ConfigurarAutoMapper()
        {
            var config = new MapperConfiguration(options => {
                var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
                options.AddProfile(new AutoMapperProfiles(geometryFactory));
            });

            return config.CreateMapper();
        }

        protected ControllerContext ConstruirControllerContext() {
            var usuario = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                        new Claim(ClaimTypes.Name, usuarioPorDefectoEmail),
                        new Claim(ClaimTypes.Email, usuarioPorDefectoEmail),
                        new Claim(ClaimTypes.NameIdentifier, usuarioPorDefectoId)
                    }));

            return new ControllerContext() {
                HttpContext = new DefaultHttpContext()
                {
                    User = usuario
                }
            };
        }

        protected WebApplicationFactory<Startup> ConstruirWebApplicationFactory(string nombreDb,
            bool ignorarSeguridad = true)
        {
            var factory = new WebApplicationFactory<Startup>();

            factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var descriptorDBContext = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                    if (descriptorDBContext != null)
                        services.Remove(descriptorDBContext);

                    services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(nombreDb));

                    if (ignorarSeguridad)
                    {
                        services.AddSingleton<IAuthorizationHandler, AllowAnonymousHandler>();

                        services.AddControllers(options => { options.Filters.Add(new UsuarioFalsoFiltro()); });
                    }
                });
            });

            return factory;
        }
    }
}
