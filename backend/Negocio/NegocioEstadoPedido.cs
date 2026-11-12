using Datos;
using Entidades;

namespace Negocio;

public class NegocioEstadoPedido
{
    public const string CodigoPendiente = "PENDIENTE";

    private readonly DaoEstadoPedido daoEstadoPedido;

    public NegocioEstadoPedido(DaoEstadoPedido daoEstado) => daoEstadoPedido = daoEstado;

    public async Task<List<EstadoPedido>> obtenerEstadoPedido()
    {
        return await daoEstadoPedido.obtenerEstadoPedido();
    }

    public async Task<EstadoPedido?> obtenerPorCodigo(string? codigo)
    {
        var estados = await obtenerEstadoPedido();
        return estados.Find(e => string.Equals(e.Codigo, codigo?.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
