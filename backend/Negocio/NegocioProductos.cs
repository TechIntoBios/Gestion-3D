using Datos;
using Entidades;

namespace Negocio; 

public class NegocioProductos
{
    private readonly DaoProductos daoProductos; 

    public NegocioProductos(DaoProductos productos) => daoProductos = productos;

    public async Task<bool> agregarProducto(int id_categoria, String nombre, double precio)
    {
        var productos = new Productos();
        productos.Id_categoria = id_categoria; 
        productos.Nombre = nombre; 
        productos.Precio = precio;

        if(!await daoProductos.existeProducto(productos))
        {
            return await daoProductos.agreagrProducto(productos);
        }

        return true;
    }


    public async Task<List<Productos>> obtenerProductos()
    {
        return await daoProductos.ListarTodosLosProductos();
    }
}