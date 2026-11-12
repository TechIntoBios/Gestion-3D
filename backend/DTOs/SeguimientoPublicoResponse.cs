namespace DTOs;

public class SeguimientoPublicoResponse
{
    public ClientePublicoResponse cliente { get; set; } = new();
    public PedidoResponse? pedido { get; set; }
}
