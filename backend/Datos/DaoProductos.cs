using Entidades;
using Npgsql;
using NpgsqlTypes;

namespace Datos;

public class DaoProductos
{
    private readonly AccesoDatos accesoDatos;
    private const string Productos = "sp_AgregarProducto";
    private const string existe_producto = "SELECT EXISTS (SELECT 1 FROM Productos WHERE Id_Categoria_Prod = @id_categoria AND LOWER(Nombre_Prod) = LOWER(@nombre))";
    private const string consulta = "Select Id_Productos_Prod, Nombre_Prod, Id_Categoria_Prod, Precio_Prod, Activo_Prod FROM Productos";

    public DaoProductos (AccesoDatos datos) => accesoDatos = datos;
    
    public async Task<bool> agreagrProducto (Productos productos)
    {
        var command = new NpgsqlCommand();
        armarParametrosAgregarProducto(command,productos);
        return await accesoDatos.ejecutarProcedimientoAlmacenado(Productos, command) != 0;
    }

    public async Task<bool> existeProducto(Productos productos)
    {
        var command = new NpgsqlCommand(); 
        armarParametrosAgregarProducto(command, productos);  
        return await accesoDatos.existe(existe_producto, command); 
    }

    public void armarParametrosAgregarProducto(NpgsqlCommand command, Productos productos)
    {
        // Los nombres tienen que ser iguales a los del procedimiento en PostgreSQL (en minúscula y sin @),
        // porque Npgsql los manda como argumentos con nombre: sp_AgregarProducto("id_categoria" := ...).
        command.Parameters.Add("id_categoria", NpgsqlDbType.Integer).Value = productos.Id_categoria;
        command.Parameters.Add("nombre",NpgsqlDbType.Varchar,100).Value = productos.Nombre;
        command.Parameters.Add("precio",NpgsqlDbType.Numeric).Value = Convert.ToDecimal(productos.Precio);
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