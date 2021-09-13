using System.Collections.Generic;

namespace PeliculasApi.DTOs
{
    public class PeliculaDetallesDto : PeliculaDto
    {
        public List<GeneroDto> Generos { get; set; }
        public List<ActorPeliculaDetalleDto> Actores { get; set; }
    }
}
