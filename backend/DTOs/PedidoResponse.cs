namespace DTOs;

public class PedidoResponse
{
    public Guid idpedido { get; set; }
    public int numero { get; set; }
    public DateTimeOffset fecha { get; set; }
    public string? codigoestado { get; set; }
    public string? estado { get; set; }
    public string? nota { get; set; }
    public string? seguimiento { get; set; }
    public decimal total { get; set; }
    public List<DetalleResponse> detalle { get; set; } = [];
}
