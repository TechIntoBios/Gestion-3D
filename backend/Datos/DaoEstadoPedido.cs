using Entidades;
using Microsoft.AspNetCore.Connections;
using Npgsql;

namespace Datos; 

public class DaoEstadoPedido
{
    private readonly AccesoDatos accesoDatos; 
    private const string consulta = "SELECT Id_EstadoPedido_EST, Codigo_EST, Estado_EST FROM EstadoPedido"; 

    public DaoEstadoPedido(AccesoDatos datos) => accesoDatos = datos; 

    public async Task<List<EstadoPedido>> obtenerEstadoPedido()
    {
        var estadoPedidos = new List<EstadoPedido>();

        try
        {
            await using var connection = await accesoDatos.obtenerConexion();
            await using var command = new NpgsqlCommand(consulta,connection);
            await using var reader = await command.ExecuteReaderAsync();

            while(await reader.ReadAsync())
            {
                var estados = new EstadoPedido(); 
                estados.IdEstadoPedido = reader.GetInt32(0);
                estados.Codigo = reader.IsDBNull(1) ? "No contiene" : reader.GetString(1);
                estados.Estado = reader.IsDBNull(2) ? "No contiene" : reader.GetString(2); 
                estadoPedidos.Add(estados); 
            }

            return estadoPedidos; 
        }
        catch (NpgsqlException e)
        {
            throw new Exception("No hay estados que mostrar", e); 
        }
    }
}