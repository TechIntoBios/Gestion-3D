using Entidades;
using Npgsql;
using NpgsqlTypes;

namespace Datos;

public class DaoProductos
{
    private readonly AccesoDatos accesoDatos;
    private const string Productos = "sp_AgregarProducto";
    private const string existe_producto = "SELECT EXISTS (SELECT 1 FROM Productos WHERE Id_Categoria_Prod = @ID_CATEGORIA AND LOWER(Nombre_Prod) = LOWER(@NOMBRE)"; 
    private const String consulta = "Select Id_Productos_Prod, Nombre_Prod, Id_Categoria_Prod, Precio_Prod, Activo_Prod FROM Productos";

    public DaoProductos (AccesoDatos datos) => accesoDatos = datos;
    
    public async Task<bool> agreagrProducto (Productos productos)
    {
        var command = new NpgsqlCommand();
        armarParametrosAgregarProducto(command,productos);
        return await accesoDatos.ejecutarProcedimientoAlmacenado(command,Productos) != 0;
    }

    public async Task<bool> existeProducto(Productos productos)
    {
        var command = new NpgsqlCommand(); 
        command.Parameters.Add("@ID_CATEGORIA", NpgsqlDbType.Integer).Value = productos.Id_categoria; 
        command.Parameters.Add("@NOMBRE", NpgsqlDbType.Varchar, 100).Value = productos.Nombre; 
        return await accesoDatos.existe(existe_producto, command); 
    }

    public void armarParametrosAgregarProducto(NpgsqlCommand command, Productos productos)
    {
        command.Parameters.Add("@ID_CATEGORIA", NpgsqlDbType.Integer).Value = productos.Id_categoria;
        command.Parameters.Add("@NOMBRE",NpgsqlDbType.Varchar,100).Value = productos.Nombre;
        command.Parameters.Add("@PRECIO",NpgsqlDbType.Numeric).Value = productos.Precio;
    }


    public async Task<List<Productos>> ListarTodosLosProductos()
    {
        var Lista = new List<Productos>();
        await using var connection = await accesoDatos.obtenerConexion();
        await using var command = new NpgsqlCommand(consulta,connection);
        await using var reader = await command.ExecuteReaderAsync();

        while(await reader.ReadAsync())
        {
            var Productos = new Productos();

            Productos.Id_producto = reader.GetInt32(0);
            Productos.Nombre = reader.IsDBNull(1) ? "No contiene" : reader.GetString(1);
            Productos.Id_categoria = reader.GetInt32(2);
            Productos.Precio = Convert.ToDouble(reader.GetDecimal(3));
            Productos.Activo = Convert.ToBoolean(reader.GetBoolean(4));
            Lista.Add(Productos);
        }

        return Lista;
    }
}