using Entidades;
using Npgsql;
using NpgsqlTypes;

namespace Datos;

public class DaoProductos
{
    private readonly AccesoDatos accesoDatos;
    private const string Productos = "sp_AgregarProducto";
    private const string function_actualizar = "SELECT fn_ActualizarProducto(@ID_PRODUCTO, @ID_CATEGORIA, @NOMBRE, @PRECIO, @ACTIVO)";
    private const string existe_producto = "SELECT EXISTS (SELECT 1 FROM Productos WHERE Id_Categoria_Prod = @id_categoria AND LOWER(Nombre_Prod) = LOWER(@nombre))";
    private const string consulta = "SELECT p.Id_Productos_Prod, p.Nombre_Prod, p.Id_Categoria_Prod, p.Precio_Prod, p.Activo_Prod FROM Productos p";

    // Un producto se puede vender solo si él y su categoría están activos.
    private const string condicion_vendible = " JOIN Categorias c ON c.Id_Categoria_Cat = p.Id_Categoria_Prod WHERE p.Activo_Prod = TRUE AND c.Activo_Cat = TRUE";
    private const string consulta_vendibles = consulta + condicion_vendible;
    private const string ids_vendibles = "SELECT p.Id_Productos_Prod FROM Productos p" + condicion_vendible + " AND p.Id_Productos_Prod = ANY(@IDS)";

    public DaoProductos(AccesoDatos datos) => accesoDatos = datos;

    public async Task agregarProducto(Productos productos)
    {
        var command = new NpgsqlCommand();
        armarParametrosAgregarProducto(command, productos);
        await accesoDatos.ejecutarProcedimientoAlmacenado(Productos, command);
    }

    // Devuelve OK, NO_EXISTE, DUPLICADO, CATEGORIA_NO_EXISTE o CATEGORIA_INACTIVA.
    public async Task<string> actualizar(Productos productos)
    {
        await using var connection = await accesoDatos.obtenerConexion();
        await using var command = new NpgsqlCommand(function_actualizar, connection);
        command.Parameters.Add("@ID_PRODUCTO", NpgsqlDbType.Integer).Value = productos.Id_producto;
        command.Parameters.Add("@ID_CATEGORIA", NpgsqlDbType.Integer).Value = productos.Id_categoria;
        command.Parameters.Add("@NOMBRE", NpgsqlDbType.Varchar, 100).Value = productos.Nombre;
        command.Parameters.Add("@PRECIO", NpgsqlDbType.Numeric).Value = productos.Precio;
        command.Parameters.Add("@ACTIVO", NpgsqlDbType.Boolean).Value = productos.Activo;

        return (string)(await command.ExecuteScalarAsync())!;
    }

    public async Task<bool> existeProducto(Productos productos)
    {
        var command = new NpgsqlCommand();
        armarParametrosAgregarProducto(command, productos);
        return await accesoDatos.existe(existe_producto, command);
    }

    public void armarParametrosAgregarProducto(NpgsqlCommand command, Productos productos)
    {
        command.Parameters.Add("id_categoria", NpgsqlDbType.Integer).Value = productos.Id_categoria;
        command.Parameters.Add("nombre", NpgsqlDbType.Varchar, 100).Value = productos.Nombre;
        command.Parameters.Add("precio", NpgsqlDbType.Numeric).Value = productos.Precio;
    }

    public Task<List<Productos>> ListarTodosLosProductos(bool soloVendibles) =>
        accesoDatos.consultar(soloVendibles ? consulta_vendibles : consulta, reader => new Productos
        {
            Id_producto = reader.GetInt32(0),
            Nombre = reader.IsDBNull(1) ? "No contiene" : reader.GetString(1),
            Id_categoria = reader.GetInt32(2),
            Precio = reader.GetDecimal(3),
            Activo = reader.GetBoolean(4)
        });

    // De los ids pedidos, devuelve los que se pueden vender.
    public async Task<HashSet<int>> obtenerVendibles(int[] ids)
    {
        var vendibles = await accesoDatos.consultar(
            ids_vendibles,
            reader => reader.GetInt32(0),
            parametros => parametros.Add("@IDS", NpgsqlDbType.Array | NpgsqlDbType.Integer).Value = ids);

        return vendibles.ToHashSet();
    }
}
