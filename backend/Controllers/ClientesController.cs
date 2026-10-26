using System.Reflection.Metadata.Ecma335;
using DTOs;
using Microsoft.AspNetCore.Mvc;
using Negocio;

namespace Controllers; 

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly NegocioClientes negocioClientes; 
    private readonly NegocioPedidos negocioPedidos; 
    private readonly NegocioDetallePedido negocioDetallePedido; 

    public ClientesController(NegocioClientes clientes, NegocioPedidos pedidos, NegocioDetallePedido detalle)
    {
        negocioClientes = clientes; 
        negocioPedidos = pedidos; 
        negocioDetallePedido = detalle; 
    }

    [HttpPost("guardar")]
    public async Task<IActionResult> agregarCliente([FromBody] CompraRequest request)
    {
        // String uuidcliente = await negocioClientes.agregarCliente(request.cliente!.nombre!, request.cliente!.apellido!, request.cliente!.correo!, request.cliente!.zona!, request.cliente!.telefono!);
        // if(uuidcliente == "" || uuidcliente == null || uuidcliente == string.Empty)
        // {
        //     return BadRequest(new {mensaje = "Error al registrar al cliente"}); 
        // }

        // String uuidpedido = await negocioPedidos.agregarPedido(Guid.Parse(uuidcliente), request.pedido!.idestado, request.pedido!.nota!, request.pedido!.Seguimiento!); 
        // if(uuidpedido == "" || uuidpedido == null || uuidpedido == string.Empty)
        // {
        //     return BadRequest(new {mensaje = "Error al registrar el pedido"});
        // }


        return Ok(); 
        // return Created(uuidpedido, "Pedido creado correctamente");
    }
}