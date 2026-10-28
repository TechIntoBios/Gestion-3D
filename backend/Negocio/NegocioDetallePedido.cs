using Datos;
using Entidades;
using Npgsql;

namespace Negocio; 

public class NegocioDetallePedido
{
    public DaoDetallePedidos daoDetallePedidos; 

    public NegocioDetallePedido(DaoDetallePedidos detallePedidos) => daoDetallePedidos = detallePedidos; 

    // public async Task<bool> agregarDetallePedido(Guid idpedido, int idproducto, String nombre, int cantidad, double precio)
    // {
    //     var detalle = new DetallePedidos(); 
    //     detalle.IdPedido = idpedido; 
    //     detalle.IdProducto = idproducto; 
    //     detalle.Nombre = nombre; 
    //     detalle.Cantidad = cantidad; 
    //     detalle.Precio = precio; 

    //     return await daoDetallePedidos.agregarDetallePedido(detalle); 
    // }

    public async Task agregarDetallePedido(Guid idpedido, int idproducto, String nombre, int cantidad, double precio , NpgsqlConnection conexion, NpgsqlTransaction transaccion)
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