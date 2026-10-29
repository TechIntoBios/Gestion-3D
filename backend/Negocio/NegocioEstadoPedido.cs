using Datos;
using Entidades;

namespace Negocio; 

public class NegocioEstadoPedido
{
    private readonly DaoEstadoPedido daoEstadoPedido; 

    public NegocioEstadoPedido(DaoEstadoPedido daoEstado) => daoEstadoPedido = daoEstado; 

    public async Task<List<EstadoPedido>> obtenerEstadoPedido()
    {
        return await daoEstadoPedido.obtenerEstadoPedido(); 
    }
}