namespace DTOs; 

public class CompraRequest{
    public ClientesRequest? cliente {get; set;}
    public PedidoResquest? pedido {get; set;}
    public List<DetalleResquest>? detalle {get; set;}
}