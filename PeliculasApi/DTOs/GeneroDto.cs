using System.ComponentModel.DataAnnotations;

namespace PeliculasApi.DTOs
{
    public class GeneroDto
    {
        public int Id { get; set; }
        [Required]
        [StringLength(40)]
        public string Nombre { get; set; }
    }
}
