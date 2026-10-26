using Datos;
using Entidades;
using Npgsql;

namespace Negocio;

public class NegocioPedidos
{

    private readonly DaoPedidos daoPedidos;

    public NegocioPedidos(DaoPedidos DaoPedidos) => daoPedidos = DaoPedidos;

    // public async Task<String>agregarPedido(Guid IdCliente, int IdEstadoPedido, String Nota, String Seguimiento)
    // {
    //     var pedido = new Pedidos();

    //     pedido.IdCliente = IdCliente;
    //     pedido.IdEstadoPedido = IdEstadoPedido;
    //     pedido.Nota = Nota;
    //     pedido.Seguimiento = Seguimiento;

    //     return await daoPedidos.agregarPedido(pedido);
    // }
    
    public async Task<Guid> agregarPedido(Guid idCliente, int idEstadoPedido, String? nota, String? seguimiento, NpgsqlConnection conexion, NpgsqlTransaction transaccion)
    {
        var pedido = new Pedidos(); 
        
        pedido.IdCliente = idCliente;
        pedido.IdEstadoPedido = idEstadoPedido;
        pedido.Nota = nota!;
        pedido.Seguimiento = seguimiento!; 

        return await daoPedidos.agregarPedido(pedido, conexion, transaccion);
    }
}