using DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly NegocioProductos negocioProductos;

    public ProductosController(NegocioProductos negocioProductos)
    {
        this.negocioProductos = negocioProductos;
    }

    [HttpGet("listar_productos")]
    public async Task<IActionResult> GetProductos([FromQuery] bool todos = false)
    {
        var productos = await negocioProductos.obtenerProductos(todos);
        return Ok(productos);
    }

    [HttpPost("Agregar")]
    public async Task<IActionResult> agregarProducto([FromBody] ProductoRequest request)
    {
        await negocioProductos.agregarProducto(request);
        return Ok(new { mensaje = "Producto agregado correctamente" });
    }

    [HttpPut("{idProducto:int}")]
    public async Task<IActionResult> actualizarProducto(int idProducto, [FromBody] ProductoRequest request)
    {
        if (!await negocioProductos.actualizar(idProducto, request))
        {
            return NotFound(new { mensaje = "El producto no existe." });
        }

        return Ok(new { mensaje = "Producto guardado correctamente." });
    }
}
