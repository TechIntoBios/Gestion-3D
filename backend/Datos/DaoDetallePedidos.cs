using Entidades;
using Npgsql;
using NpgsqlTypes;

namespace Datos; 

public class DaoDetallePedidos
{
    private const string detalle_Pedido = "CALL sp_AgregarDetallePedido(@ID_PEDIDO, @ID_PRODUCTO, @NOMBRE, @CANTIDAD, @PRECIO)"; 

    public DaoDetallePedidos() { }

    
    public async Task agregarDetallePedido(DetallePedidos detallePedidos, NpgsqlConnection conexion, NpgsqlTransaction transaccion)
    {
        await using var command = new NpgsqlCommand(detalle_Pedido, conexion, transaccion);
        armarParametrosAgregarDetallePedido(command, detallePedidos);

        await command.ExecuteNonQueryAsync();
    }

    public void armarParametrosAgregarDetallePedido(NpgsqlCommand command, DetallePedidos detallePedidos)
    {
        command.Parameters.Add("@ID_PEDIDO", NpgsqlDbType.Uuid).Value = detallePedidos.IdPedido;
        command.Parameters.Add("@ID_PRODUCTO", NpgsqlDbType.Integer).Value = detallePedidos.IdProducto;
        command.Parameters.Add("@NOMBRE", NpgsqlDbType.Varchar, 100).Value = (object?)detallePedidos.Nombre ?? DBNull.Value; 
        command.Parameters.Add("@CANTIDAD", NpgsqlDbType.Integer).Value = detallePedidos.Cantidad; 
        command.Parameters.Add("@PRECIO", NpgsqlDbType.Numeric).Value = detallePedidos.Precio; 
    }

}