using Entidades;

namespace DTOs;

public class SeguimientoResponse
{
    public Clientes? cliente { get; set; }
    public List<PedidoResponse> pedidos { get; set; } = [];
}

public class PedidoResponse
{
    public Guid idpedido { get; set; }
    public int numero { get; set; }
    public DateTimeOffset fecha { get; set; }
    public String? codigoestado { get; set; }
    public String? estado { get; set; }
    public String? nota { get; set; }
    public String? seguimiento { get; set; }
    public decimal total { get; set; }
    public List<DetalleResponse> detalle { get; set; } = [];
}

public class DetalleResponse
{
    public String? nombre { get; set; }
    public int cantidad { get; set; }
    public decimal precio { get; set; }
    public decimal importe { get; set; }
}
