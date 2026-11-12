using Entidades;

namespace DTOs;

public class SeguimientoResponse
{
    public Clientes? cliente { get; set; }
    public List<PedidoResponse> pedidos { get; set; } = [];
}
