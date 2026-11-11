namespace DTOs;
public class SeguimientoPublicoResponse
{
    public ClientePublico cliente { get; set; } = new();
    public PedidoResponse? pedido { get; set; }
}

public class ClientePublico
{
    public String? nombre { get; set; }
    public String? apellido { get; set; }
    public String? correo { get; set; }
    public String? telefono { get; set; }
    public String? zona { get; set; }
}
