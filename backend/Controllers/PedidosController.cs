using DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Negocio;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly NegocioPedidos negocioPedidos;

    public PedidosController(NegocioPedidos pedidos) => negocioPedidos = pedidos;

    [HttpGet("por_telefono/{telefono}")]
    public async Task<IActionResult> buscarPorTelefono(string telefono)
    {
        var seguimiento = await negocioPedidos.buscarPorTelefono(telefono);
        if (seguimiento is null)
        {
            return NotFound(new { mensaje = "No hay ningún cliente con ese teléfono." });
        }

        return Ok(seguimiento);
    }

    [AllowAnonymous]
    [EnableRateLimiting(NegocioLimitesApi.Publico)]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [HttpGet("publico/{idPedido:guid}")]
    public async Task<IActionResult> obtenerPublico(Guid idPedido)
    {
        var seguimiento = await negocioPedidos.obtenerPublico(idPedido);
        if (seguimiento is null)
        {
            return NotFound(new { mensaje = "No encontramos ese pedido. Revisá que el link esté completo." });
        }

        return Ok(seguimiento);
    }

    [HttpPut("{idPedido:guid}/estado")]
    public async Task<IActionResult> cambiarEstado(Guid idPedido, [FromBody] CambioEstadoRequest request)
    {
        var resultado = await negocioPedidos.cambiarEstado(idPedido, request.codigoestado, request.seguimiento);
        if (resultado is null)
        {
            return NotFound(new { mensaje = "El pedido no existe." });
        }

        var (pedido, mailEnviado) = resultado.Value;
        var mensaje = mailEnviado ? "Cambios guardados. Le avisamos al cliente por mail." : "Cambios guardados.";
        return Ok(new { pedido, mailEnviado, mensaje });
    }
}
