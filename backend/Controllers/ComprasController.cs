using DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class ComprasController : ControllerBase
{
    private readonly NegocioCompras negocioCompras;

    public ComprasController(NegocioCompras negocioCompras)
    {
        this.negocioCompras = negocioCompras;
    }

    [HttpPost("guardar")]
    public async Task<IActionResult> registrarCompra([FromBody] CompraRequest request)
    {
        try
        {
            var idPedido = await negocioCompras.registrarCompra(request);
            return StatusCode(StatusCodes.Status201Created, new { idPedido, mensaje = "Compra creada correctamente." });
        }
        catch (ArgumentException e)
        {
            return BadRequest(new { mensaje = e.Message });
        }
    }
}
