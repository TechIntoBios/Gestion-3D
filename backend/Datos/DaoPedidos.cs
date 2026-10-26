using System.Data;
using System.Runtime.InteropServices;
using Entidades;
using Npgsql;
using NpgsqlTypes; 

namespace Datos; 

public class DaoPedidos
{
    private readonly AccesoDatos accesoDatos; 
    private const string Pedido = "sp_AgregarPedido"; 
    private const string function_pedido = "SELECT fn_AgregarPedido(@ID_CLIENTE, @ID_ESTADO, @NOTA, @SEGUIMIENTO)"; 
    private const string function_pedido_compra = "SELECT fn_AgregarPedido(@ID_CLIENTE, @ID_ESTADO, @NOTA, @SEGUIMIENTO, @FECHA)";

    public DaoPedidos(AccesoDatos datos) => accesoDatos = datos;

    // public async Task<String> agregarPedido(Pedidos pedidos)
    // {
    //     var command = new NpgsqlCommand(); 
    //     armarParametrosAgregarPedido(command, pedidos); 

    //     object? result = await accesoDatos.ejecutarFunction(command, function_pedido);
    //     if(result == null || result == DBNull.Value)
    //     {
    //         throw new Exception("No se pudo registrar el pedido.");
    //     }
    //     return (String)result; 
    // }

    public async Task<Guid> agregarPedido(Pedidos pedidos, NpgsqlConnection conexion, NpgsqlTransaction transaccion)
    {
        await using var command = new NpgsqlCommand(function_pedido_compra, conexion, transaccion);
        armarParametrosAgregarPedido(command, pedidos);

        var result = await command.ExecuteScalarAsync();
        if (result is not Guid idPedido)
        {
            throw new InvalidOperationException("La función no devolvió el UUID del pedido.");
        }

        return idPedido;
    }
    
    public void armarParametrosAgregarPedido(NpgsqlCommand command, Pedidos pedidos)
    {
        command.Parameters.Add("@ID_CLIENTE", NpgsqlDbType.Uuid).Value = pedidos.IdCliente; 
        command.Parameters.Add("@ID_ESTADO", NpgsqlDbType.Integer).Value = pedidos.IdEstadoPedido; 
        command.Parameters.Add("@NOTA", NpgsqlDbType.Varchar, 300).Value = (object?)pedidos.Nota ?? DBNull.Value; 
        command.Parameters.Add("@SEGUIMIENTO", NpgsqlDbType.Varchar, 300).Value = (object?)pedidos.Seguimiento ?? DBNull.Value; 
    }
}