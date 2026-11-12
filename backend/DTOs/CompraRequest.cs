namespace DTOs; 

public class CompraRequest{
    public ClientesRequest? cliente {get; set;}
    public PedidoRequest? pedido {get; set;}
    public List<DetalleRequest>? detalle {get; set;}
}