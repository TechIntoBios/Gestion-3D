using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly NegocioClientes negocioClientes;

    public ClientesController(NegocioClientes clientes) => negocioClientes = clientes;

    [HttpGet("por_telefono/{telefono}")]
    public async Task<IActionResult> buscarPorTelefono(string telefono)
    {
        try
        {
            var cliente = await negocioClientes.buscarPorTelefono(telefono);
            if (cliente is null)
            {
                return NotFound(new { mensaje = "No hay ningún cliente con ese teléfono." });
            }

            return Ok(cliente);
        }
        catch (ArgumentException e)
        {
            return BadRequest(new { mensaje = e.Message });
        }
    }
}
