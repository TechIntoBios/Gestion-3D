using System.Data;
using System.Runtime.InteropServices;
using DTOs;
using Entidades;
using Npgsql;
using NpgsqlTypes; 

namespace Datos; 

public class DaoPedidos
{
    private readonly AccesoDatos accesoDatos; 
    private const string Pedido = "sp_AgregarPedido"; 
    private const string function_pedido = "SELECT fn_AgregarPedido(@ID_CLIENTE, @ID_ESTADO, @NOTA, @SEGUIMIENTO)";
    private const string numero_pedido = "SELECT NumeroPedido_Ped FROM Pedidos WHERE Id_Pedido_Ped = @ID_PEDIDO";
    private const string telefono_del_pedido = "SELECT c.Telefono_Cli FROM Pedidos p JOIN Clientes c ON c.Id_cliente_Cli = p.Id_cliente_Ped WHERE p.Id_Pedido_Ped = @ID_PEDIDO";
    private const string function_estado = "SELECT TELEFONO, CAMBIO_ESTADO, HUBO_CAMBIO FROM fn_ActualizarEstadoPedido(@ID_PEDIDO, @ID_ESTADO, @SEGUIMIENTO)";

    private const string pedidos_por_telefono = @"
        SELECT p.Id_Pedido_Ped, p.NumeroPedido_Ped, p.FechaPedido_Ped, e.Codigo_EST, e.Estado_EST,
               p.NotaPedido_Ped, p.Seguimiento_Ped, d.Nombre_Dped, d.Cantidad_Dped, d.Precio_Dped, d.Importe_Dped
        FROM Pedidos p
        JOIN Clientes c ON c.Id_cliente_Cli = p.Id_cliente_Ped
        JOIN EstadoPedido e ON e.Id_EstadoPedido_EST = p.Id_EstadoPedido_Ped
        LEFT JOIN DetallePedido d ON d.Id_Pedido_Dped = p.Id_Pedido_Ped
        WHERE c.Telefono_Cli = @TELEFONO
        ORDER BY p.NumeroPedido_Ped DESC, d.Id_DetallePedido_Dped";

    public DaoPedidos(AccesoDatos datos) => accesoDatos = datos;


    public async Task<Guid> agregarPedido(Pedidos pedidos, NpgsqlConnection conexion, NpgsqlTransaction transaccion)
    {
        await using var command = new NpgsqlCommand(function_pedido, conexion, transaccion);
        armarParametrosAgregarPedido(command, pedidos);

        var result = await command.ExecuteScalarAsync();
        if (result is not Guid idPedido)
        {
            throw new InvalidOperationException("La función no devolvió el UUID del pedido.");
        }

        return idPedido;
    }

    public async Task<int> obtenerNumero(Guid idPedido, NpgsqlConnection conexion, NpgsqlTransaction transaccion)
    {
        await using var command = new NpgsqlCommand(numero_pedido, conexion, transaccion);
        command.Parameters.Add("@ID_PEDIDO", NpgsqlDbType.Uuid).Value = idPedido;
        return (int)(await command.ExecuteScalarAsync())!;
    }

    public async Task<String?> obtenerTelefonoCliente(Guid idPedido)
    {
        var command = new NpgsqlCommand();
        command.Parameters.Add("@ID_PEDIDO", NpgsqlDbType.Uuid).Value = idPedido;
        return await accesoDatos.ejecutarScalar(telefono_del_pedido, command) as String;
    }

    public async Task<(String telefono, bool cambioEstado, bool huboCambio)?> actualizarEstado(Guid idPedido, int idEstado, String? seguimiento)
    {
        await using var connection = await accesoDatos.obtenerConexion();
        await using var command = new NpgsqlCommand(function_estado, connection);
        command.Parameters.Add("@ID_PEDIDO", NpgsqlDbType.Uuid).Value = idPedido;
        command.Parameters.Add("@ID_ESTADO", NpgsqlDbType.Integer).Value = idEstado;
        command.Parameters.Add("@SEGUIMIENTO", NpgsqlDbType.Varchar, 300).Value = (object?)seguimiento ?? DBNull.Value;
        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        return (reader.GetString(0), reader.GetBoolean(1), reader.GetBoolean(2));
    }

    public async Task<List<PedidoResponse>> listarPorTelefono(String telefono)
    {
        var pedidos = new List<PedidoResponse>();

        await using var connection = await accesoDatos.obtenerConexion();
        await using var command = new NpgsqlCommand(pedidos_por_telefono, connection);
        command.Parameters.Add("@TELEFONO", NpgsqlDbType.Varchar, 20).Value = telefono;
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var idPedido = reader.GetGuid(0);
            var pedido = pedidos.LastOrDefault();

            if (pedido is null || pedido.idpedido != idPedido)
            {
                pedido = new PedidoResponse
                {
                    idpedido = idPedido,
                    numero = reader.GetInt32(1),
                    fecha = reader.GetFieldValue<DateTimeOffset>(2),
                    codigoestado = reader.GetString(3),
                    estado = reader.GetString(4),
                    nota = reader.IsDBNull(5) ? null : reader.GetString(5),
                    seguimiento = reader.IsDBNull(6) ? null : reader.GetString(6)
                };
                pedidos.Add(pedido);
            }

           
            if (!reader.IsDBNull(7))
            {
                var detalle = new DetalleResponse
                {
                    nombre = reader.GetString(7),
                    cantidad = reader.GetInt32(8),
                    precio = reader.GetDecimal(9),
                    importe = reader.GetDecimal(10)
                };
                pedido.detalle.Add(detalle);
                pedido.total += detalle.importe;
            }
        }

        return pedidos;
    }

    public void armarParametrosAgregarPedido(NpgsqlCommand command, Pedidos pedidos)
    {
        command.Parameters.Add("@ID_CLIENTE", NpgsqlDbType.Uuid).Value = pedidos.IdCliente; 
        command.Parameters.Add("@ID_ESTADO", NpgsqlDbType.Integer).Value = pedidos.IdEstadoPedido; 
        command.Parameters.Add("@NOTA", NpgsqlDbType.Varchar, 300).Value = (object?)pedidos.Nota ?? DBNull.Value; 
        command.Parameters.Add("@SEGUIMIENTO", NpgsqlDbType.Varchar, 300).Value = (object?)pedidos.Seguimiento ?? DBNull.Value; 
    }
}