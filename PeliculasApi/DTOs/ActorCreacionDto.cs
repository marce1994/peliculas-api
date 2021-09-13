using Microsoft.AspNetCore.Http;
using PeliculasApi.Validaciones;

namespace PeliculasApi.DTOs
{
    public class ActorCreacionDto : ActorPatchDto
    {
        [PesoArchivoValidacion(PesoMaximoEnMegaBytes: 4)]
        [TipoArchivoValidacion(GrupoTipoArchivo: GrupoTipoArchivo.Imagen)]
        public IFormFile Foto { get; set; }
    }
}