using DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly NegocioCategorias negocioCategorias;

    public CategoriasController(NegocioCategorias negocioC)
    {
        this.negocioCategorias = negocioC;
    }

    [HttpGet("listar_Categorias")]
    public async Task<IActionResult> GetCategorias([FromQuery] bool todos = false)
    {
        var categorias = await negocioCategorias.ListarCategorias(todos);
        return Ok(categorias);
    }

    [HttpPost("agregar_Categorias")]
    public async Task<IActionResult> agregarCategorias([FromBody] CategoriaRequest request)
    {
        await negocioCategorias.AgregarCategorias(request);
        return Ok(new { mensaje = "Categoria agregada correctamente" });
    }

    [HttpPut("{idCategoria:int}")]
    public async Task<IActionResult> actualizarCategoria(int idCategoria, [FromBody] CategoriaRequest request)
    {
        if (!await negocioCategorias.actualizar(idCategoria, request))
        {
            return NotFound(new { mensaje = "La categoría no existe." });
        }

        return Ok(new { mensaje = "Categoría guardada correctamente." });
    }
}
