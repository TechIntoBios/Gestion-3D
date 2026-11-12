using Datos;
using DTOs;
using Entidades;
using Npgsql;

namespace Negocio;

public class NegocioProductos
{
    private const string MensajeDuplicado = "Ya existe un producto con ese nombre en esa categoría.";

    private readonly DaoProductos daoProductos;

    public NegocioProductos(DaoProductos productos) => daoProductos = productos;

    private static Productos validar(ProductoRequest request)
    {
        var producto = new Productos
        {
            Id_categoria = request.id_categoria,
            Nombre = NegocioValidaciones.textoRequerido(request.nombre, "El nombre del producto", 100),
            Precio = request.precio,
            Activo = request.activo
        };

        if (producto.Id_categoria <= 0)
        {
            throw new ArgumentException("Elegí una categoría.");
        }

        NegocioValidaciones.precio(producto.Precio);
        return producto;
    }

    public async Task agregarProducto(ProductoRequest request)
    {
        var producto = validar(request);

        if (await daoProductos.existeProducto(producto))
        {
            throw new NegocioConflictoException(MensajeDuplicado);
        }

        try
        {
            await daoProductos.agregarProducto(producto);
        }
        catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            throw new ArgumentException("La categoría elegida no existe.", e);
        }
        catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new NegocioConflictoException(MensajeDuplicado);
        }
    }

    public async Task<bool> actualizar(int idProducto, ProductoRequest request)
    {
        var producto = validar(request);
        producto.Id_producto = idProducto;

        return await daoProductos.actualizar(producto) switch
        {
            "OK" => true,
            "NO_EXISTE" => false,
            "DUPLICADO" => throw new NegocioConflictoException(MensajeDuplicado),
            "CATEGORIA_NO_EXISTE" => throw new ArgumentException("La categoría elegida no existe."),
            "CATEGORIA_INACTIVA" => throw new ArgumentException("La categoría elegida está desactivada. Activala primero o elegí otra."),
            var otro => throw new InvalidOperationException($"fn_ActualizarProducto devolvió un resultado inesperado: {otro}.")
        };
    }

    public async Task<List<Productos>> obtenerProductos(bool todos = false)
    {
        return await daoProductos.ListarTodosLosProductos(soloVendibles: !todos);
    }

    public async Task validarVendibles(IEnumerable<int> idsProductos)
    {
        var ids = idsProductos.Distinct().ToArray();
        var vendibles = await daoProductos.obtenerVendibles(ids);

        if (ids.Any(id => !vendibles.Contains(id)))
        {
            throw new ArgumentException("Alguno de los productos ya no está disponible. Actualizá la página y volvé a elegirlos.");
        }
    }
}
