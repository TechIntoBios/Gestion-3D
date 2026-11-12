namespace DTOs;

public class ProductoRequest
{
    public int id_categoria { get; set; }
    public string? nombre { get; set; }
    public decimal precio { get; set; }
    public bool activo { get; set; } = true;
}
