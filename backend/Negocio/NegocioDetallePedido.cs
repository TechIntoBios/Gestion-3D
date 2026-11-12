using Datos;
using Entidades;
using Npgsql;

namespace Negocio; 

public class NegocioDetallePedido
{
    public DaoDetallePedidos daoDetallePedidos; 

    public NegocioDetallePedido(DaoDetallePedidos detallePedidos) => daoDetallePedidos = detallePedidos; 


    public async Task agregarDetallePedido(Guid idpedido, int idproducto, String nombre, int cantidad, decimal precio, NpgsqlConnection conexion, NpgsqlTransaction transaccion)
    {
        var detalle = new DetallePedidos();
        detalle.IdPedido = idpedido;
        detalle.IdProducto = idproducto;
        detalle.Nombre = nombre;
        detalle.Cantidad = cantidad;
        detalle.Precio = precio;

        await daoDetallePedidos.agregarDetallePedido(detalle, conexion, transaccion);
    }
}