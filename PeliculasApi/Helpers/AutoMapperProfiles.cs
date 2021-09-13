using AutoMapper;
using Microsoft.AspNetCore.Identity;
using NetTopologySuite.Geometries;
using PeliculasApi.DTOs;
using PeliculasApi.Entidades;
using System.Collections.Generic;
using System.Linq;

namespace PeliculasApi.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles(GeometryFactory geometryFactory)
        {
            CreateMap<Genero, GeneroDto>().ReverseMap();
            CreateMap<GeneroCreacionDto, Genero>();

            CreateMap<Review, ReviewDto>()
                .ForMember(x => x.NombreUsuario, x => x.MapFrom(y => y.Usuario.UserName));

            CreateMap<ReviewDto, Review>();
            CreateMap<ReviewCreacionDto, Review>();

            CreateMap<IdentityUser, UsuarioDto>();

            CreateMap<SalaDeCine, SalaDeCineDto>()
                .ForMember(x => x.Latitud, x => x.MapFrom(y => y.Ubicacion.Y))
                .ForMember(x => x.Longitud, x => x.MapFrom(y => y.Ubicacion.X));

            CreateMap<SalaDeCineDto, SalaDeCine>()
                .ForMember(x => x.Ubicacion, opts => opts.MapFrom(y => geometryFactory.CreatePoint(new Coordinate(y.Longitud, y.Latitud))));

            CreateMap<SalaDeCineCreacionDto, SalaDeCine>()
                .ForMember(x => x.Ubicacion, opts => opts.MapFrom(y => geometryFactory.CreatePoint(new Coordinate(y.Longitud, y.Latitud))));

            CreateMap<Actor, ActorDto>().ReverseMap();
            CreateMap<ActorCreacionDto, Actor>()
                .ForMember(x => x.Foto, options => options.Ignore());
            CreateMap<ActorPatchDto, Actor>().ReverseMap();

            CreateMap<Pelicula, PeliculaDto>().ReverseMap();
            CreateMap<PeliculaCreacionDto, Pelicula>()
                .ForMember(x => x.Poster, options => options.Ignore())
                .ForMember(x => x.PeliculasGeneros, options => options.MapFrom(MapPeliculasGeneros))
                .ForMember(x => x.PeliculasActores, options => options.MapFrom(MapPeliculasActores));

            CreateMap<PeliculaPatchDto, Pelicula>().ReverseMap();

            CreateMap<Pelicula, PeliculaDetallesDto>()
                .ForMember(x => x.Generos, options => options.MapFrom(MapPeliculasGeneros))
                .ForMember(x => x.Actores, options => options.MapFrom(MapPeliculasActores));
        }

        private List<ActorPeliculaDetalleDto> MapPeliculasActores(Pelicula pelicula, PeliculaDetallesDto peliculaDetalleDto)
        {
            if (pelicula.PeliculasGeneros == null)
                return new List<ActorPeliculaDetalleDto>();

            return pelicula.PeliculasActores.Select(x => new ActorPeliculaDetalleDto { ActorId = x.ActorId, Personaje = x.Personaje, NombrePersona = x.Actor.Nombre }).ToList();
        }

        private List<GeneroDto> MapPeliculasGeneros(Pelicula pelicula, PeliculaDetallesDto peliculaDetalleDto)
        {
            if (pelicula.PeliculasGeneros == null)
                return new List<GeneroDto>();

            return pelicula.PeliculasGeneros.Select(x => new GeneroDto { Id = x.GeneroId, Nombre = x.Genero.Nombre }).ToList();
        }

        private List<PeliculasGeneros> MapPeliculasGeneros(PeliculaCreacionDto peliculaCreacionDto, Pelicula pelicula)
        {
            if (peliculaCreacionDto.GenerosIDs == null)
                return new List<PeliculasGeneros>();

            return peliculaCreacionDto.GenerosIDs.Select(id => new PeliculasGeneros { GeneroId = id}).ToList();
        }

        private List<PeliculasActores> MapPeliculasActores(PeliculaCreacionDto peliculaCreacionDto, Pelicula pelicula)
        {
            if (peliculaCreacionDto.Actores == null)
                return new List<PeliculasActores>();

            return peliculaCreacionDto.Actores.Select(actor => new PeliculasActores { ActorId = actor.ActorId, Personaje = actor.Personaje }).ToList();
        }
    }
}
