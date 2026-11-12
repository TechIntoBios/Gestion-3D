using Entidades;

namespace Datos;

public class DaoEstadoPedido
{
    private readonly AccesoDatos accesoDatos;
    private const string consulta = "SELECT Id_EstadoPedido_EST, Codigo_EST, Estado_EST FROM EstadoPedido";

    public DaoEstadoPedido(AccesoDatos datos) => accesoDatos = datos;

    public Task<List<EstadoPedido>> obtenerEstadoPedido() =>
        accesoDatos.consultar(consulta, reader => new EstadoPedido
        {
            IdEstadoPedido = reader.GetInt32(0),
            Codigo = reader.IsDBNull(1) ? "No contiene" : reader.GetString(1),
            Estado = reader.IsDBNull(2) ? "No contiene" : reader.GetString(2)
        });
}
