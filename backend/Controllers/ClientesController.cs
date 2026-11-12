using DTOs;
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
        var cliente = await negocioClientes.buscarPorTelefono(telefono);
        if (cliente is null)
        {
            return NotFound(new { mensaje = "No hay ningún cliente con ese teléfono." });
        }

        return Ok(cliente);
    }

    [HttpPut("{idCliente:guid}")]
    public async Task<IActionResult> actualizar(Guid idCliente, [FromBody] ClientesRequest request)
    {
        var cliente = await negocioClientes.actualizar(idCliente, request);
        if (cliente is null)
        {
            return NotFound(new { mensaje = "El cliente no existe." });
        }

        return Ok(new { cliente, mensaje = "Datos de contacto guardados." });
    }
}
