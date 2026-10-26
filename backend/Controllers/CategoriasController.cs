using DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;
using Entidades; 

namespace Controllers;

[ApiController]
[Route("api/[controller]")]

public class CategoriasController : ControllerBase
{
    private readonly NegocioCategorias negocioCategorias;

    public CategoriasController(NegocioCategorias negocioCategorias)
    {
        this.negocioCategorias = negocioCategorias;
    }

    [HttpGet("Categorias")]
    public async Task<IActionResult> GetCategorias()
    {
        var categorias = await negocioCategorias.ListarCategorias();
        return Ok(categorias);
    }

    [HttpPost("Categorias")]

    public async Task<IActionResult> agregarCategorias([FromBody] Categorias categoria)
    {
        bool Result = await negocioCategorias.AgregarCategorias(categoria.NOMBRE);
        
        if(!Result)
        {
            return BadRequest(new {mensaje = "No se pudo agregar la categoria"});
        }
            return Ok(new {mensaje = "Categoria agregada correctamente" });
    }
}