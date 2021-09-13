using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PeliculasApi.Helpers;
using PeliculasApi.Validaciones;
using System.Collections.Generic;

namespace PeliculasApi.DTOs
{
    public class PeliculaCreacionDto : PeliculaPatchDto
    {
        [PesoArchivoValidacion(4)]
        [TipoArchivoValidacion(GrupoTipoArchivo: GrupoTipoArchivo.Imagen)]
        public IFormFile Poster { get; set; }

        [ModelBinder(BinderType = typeof(TypeBinder<List<int>>))]
        public List<int> GenerosIDs { get; set; }
        [ModelBinder(BinderType = typeof(TypeBinder<List<ActorPeliculasCreacionDto>>))]
        public List<ActorPeliculasCreacionDto> Actores { get; set; }
    }
}