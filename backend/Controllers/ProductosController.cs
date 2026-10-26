using DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;
using Entidades;

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

    [HttpGet("Productos")]
    public async Task<IActionResult> GetProductos()
    {
        var productos = negocioProductos.obtenerProductos();
        return Ok(productos);
    }

    [HttpPost("Agregar")]
    public async Task<IActionResult> agregarProducto([FromBody]Productos request)
    {
        bool resultado = await negocioProductos.agregarProducto(request.Id_categoria, request.Nombre, request.Precio);
        if(!resultado)
        {
            return BadRequest(new {mensaje = "No se pudo agregar el producto." });
        }
            return Ok(new {mensaje = "Producto agregdo correctamente" });
    }
    
}