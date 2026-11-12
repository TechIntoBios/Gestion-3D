namespace DTOs; 

public class DetalleRequest
{
    public Guid idpedido {get; set;}
    public int idproducto {get; set;}
    public String? nombre {get; set;}
    public int cantidad {get; set;}
    public decimal precio {get; set;}
}