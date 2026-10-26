

using System.Data;
using System.Data.Common;
using System.Net.WebSockets;
using System.Reflection.Metadata.Ecma335;
using System.Security.AccessControl;
using System.Security.Cryptography;
using Npgsql;

namespace Datos;

public class AccesoDatos
{

    private readonly String cadena;

    public AccesoDatos(IConfiguration configuration)
    {
        cadena = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Falta la cadena de conexión 'ConnectionStrings:Impresiones3D'.");
    }

    public async Task<NpgsqlConnection> obtenerConexion()
    {
        using var connection = new NpgsqlConnection(cadena);
        try
        {
            await connection.OpenAsync();
            return connection;
        }
        catch (NpgsqlException e)
        {
            await connection.DisposeAsync();
            throw new Exception("No se pudo conectar a la base de datos", e);
        }
    }

    public async Task<int> ejecutarAccionesIUD(String consulta, NpgsqlCommand command)
    {
        try
        {
            using var connection = await obtenerConexion();
            command.Connection = connection;
            command.CommandText = consulta;

            return await command.ExecuteNonQueryAsync();
        }
        catch (NpgsqlException e)
        {
            throw new Exception("Ocurrio un error al ejecutar la accion", e);
        }
    }

    public async Task<object?> ejecutarScalar(String consulta, NpgsqlCommand command)
    {
        try
        {
            using var connection = await obtenerConexion(); 
            command.Connection = connection; 
            command.CommandText = consulta; 

            return await command.ExecuteScalarAsync(); 
        }
        catch(NpgsqlException e)
        {
            throw new Exception("Ocurrio un error al obtener el Scalar", e); 
        }
    }

    // la consulta debe de tener SELECT EXISTS (SELECT 1 ...);
    public async Task<bool> existe(String consulta, NpgsqlCommand command)
    {
        try
        {
            using var connection = await obtenerConexion();
            command.Connection = connection;
            command.CommandText = consulta;

            return (bool)(await command.ExecuteScalarAsync())!;
        }
        catch (NpgsqlException e)
        {
            throw new Exception("No se encontro coincidencia", e);
        }
    }

    public async Task<int> ejecutarProcedimientoAlmacenado(NpgsqlCommand command, String procedimiento, bool scalar = false)
    {
        try
        {
            using var connection = await obtenerConexion();
            command.Connection = connection;
            command.CommandText = procedimiento;
            command.CommandType = CommandType.StoredProcedure;

            return await command.ExecuteNonQueryAsync();
        }
        catch (NpgsqlException e)
        {
            throw new Exception("Error al ejecutar el procedimiento almacenado.", e);
        }
    }

    public async Task<object?> ejecutarFunction(NpgsqlCommand command, String consulta)
    {
        try
        {
            using var connection = await obtenerConexion();
            command.Connection = connection;
            command.CommandText = consulta;
            command.CommandType = CommandType.Text;

            return await command.ExecuteScalarAsync();
        }
        catch (NpgsqlException e)
        {
            throw new Exception("Error al ejecutar la función.", e);
        }
    }
}