using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeliculasApi.DTOs;
using PeliculasApi.Entidades;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PeliculasApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenerosController : CustomBaseController
    {
        public GenerosController(ApplicationDbContext context, IMapper mapper) : base(context, mapper)
        {
        }

        [HttpGet]
        public async Task<ActionResult<List<GeneroDto>>> Get()
        {
            return await Get<Genero, GeneroDto>();
        }

        [HttpGet("{id:int}", Name = "obtenerGenero")]
        public async Task<ActionResult<GeneroDto>> Get(int id)
        {
            return await Get<Genero, GeneroDto>(id);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] GeneroCreacionDto generoCreacionDto)
        {
            return await Post<GeneroCreacionDto, Genero, GeneroDto>(generoCreacionDto, "obtenerGenero");
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, [FromBody] GeneroCreacionDto generoCreacionDto)
        {
            return await Put<GeneroCreacionDto, Genero>(id, generoCreacionDto);
        }

        [HttpDelete("{id:int}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<ActionResult> Delete(int id) {
            return await Delete<Genero>(id);
        }
    }
}
