using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace Controllers; 

[ApiController]
[Route("api/[controller]")]
public class EstadoPedidoController : ControllerBase
{
    private readonly NegocioEstadoPedido negocioEstadoPedido;

    public EstadoPedidoController(NegocioEstadoPedido negocioEstado) => negocioEstadoPedido = negocioEstado; 

    [HttpGet("estados")]
    public async Task<IActionResult> getEstadoPedido()
    {
        var lista = await negocioEstadoPedido.obtenerEstadoPedido(); 
        return Ok(lista); 
    }
}