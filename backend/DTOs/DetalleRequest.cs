namespace DTOs; 

public class DetalleResquest
{
    public Guid idpedido {get; set;}
    public int idproducto {get; set;}
    public String? nombre {get; set;}
    public int cantidad {get; set;}
    public double precio {get; set;}
}