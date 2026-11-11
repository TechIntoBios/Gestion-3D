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
        try
        {
            var seguimiento = await negocioPedidos.buscarPorTelefono(telefono);
            if (seguimiento is null)
            {
                return NotFound(new { mensaje = "No hay ningún cliente con ese teléfono." });
            }

            return Ok(seguimiento);
        }
        catch (ArgumentException e)
        {
            return BadRequest(new { mensaje = e.Message });
        }
    }

    // tracking.html (público, sin sesión): el link lleva el UUID del pedido, que no se puede adivinar.
    // Límite por IP para que no se pueda probar ids en masa, y sin caché para que no quede guardado en proxies.
    [AllowAnonymous]
    [EnableRateLimiting(LimitesApi.Publico)]
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
        try
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
        catch (ArgumentException e)
        {
            return BadRequest(new { mensaje = e.Message });
        }
    }
}
