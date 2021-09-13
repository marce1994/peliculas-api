using System.Collections.Generic;

namespace PeliculasApi.DTOs
{
    public class PeliculasIndexDto
    {
        public List<PeliculaDto> FuturosEstrenos { get; set; }
        public List<PeliculaDto> EnCines { get; set; }
    }
}
