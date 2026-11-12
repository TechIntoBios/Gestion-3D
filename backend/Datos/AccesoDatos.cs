using System.Data;
using Npgsql;

namespace Datos;

public class AccesoDatos
{
    private readonly string cadena;

    public AccesoDatos(IConfiguration configuration)
    {
        cadena = configuration.GetConnectionString("Impresiones3D") ?? throw new InvalidOperationException("Falta la cadena de conexión 'ConnectionStrings:Impresiones3D'.");
    }

    public async Task<NpgsqlConnection> obtenerConexion()
    {
        var connection = new NpgsqlConnection(cadena);
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

    public async Task<List<T>> consultar<T>(string consulta, Func<NpgsqlDataReader, T> leer, Action<NpgsqlParameterCollection>? parametros = null)
    {
        await using var connection = await obtenerConexion();
        await using var command = new NpgsqlCommand(consulta, connection);
        parametros?.Invoke(command.Parameters);
        await using var reader = await command.ExecuteReaderAsync();

        var filas = new List<T>();
        while (await reader.ReadAsync())
        {
            filas.Add(leer(reader));
        }

        return filas;
    }

    public async Task<bool> existe(string consulta, NpgsqlCommand command)
    {
        await using var connection = await obtenerConexion();
        command.Connection = connection;
        command.CommandText = consulta;

        return (bool)(await command.ExecuteScalarAsync())!;
    }

    public async Task ejecutarProcedimientoAlmacenado(string procedimiento, NpgsqlCommand command)
    {
        await using var connection = await obtenerConexion();
        command.Connection = connection;
        command.CommandText = procedimiento;
        command.CommandType = CommandType.StoredProcedure;

        await command.ExecuteNonQueryAsync();
    }
}
