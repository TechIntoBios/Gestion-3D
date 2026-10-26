namespace Entidades;

public class DetallePedidos
{
    private int id_detallePedido;
    private Guid id_pedido;
    private int id_producto;
    private string nombre;
    private int cantidad;
    private double precio;
    private double importe;

    public DetallePedidos()
    {
        id_detallePedido = 0;
        id_pedido = Guid.Empty;
        id_producto = 0;
        nombre = "Sin definir";
        cantidad = 0;
        precio = 0;
        importe = 0;
    }

    public DetallePedidos(int id_detallePedido, Guid id_pedido, int id_producto, string nombre, int cantidad, double precio, double importe)
    {
        this.id_detallePedido = id_detallePedido;
        this.id_pedido = id_pedido;
        this.id_producto = id_producto;
        this.nombre = nombre;
        this.cantidad = cantidad;
        this.precio = precio;
        this.importe = importe;
    }

    public int IdDetallePedido
    {
        get
        {
            return id_detallePedido;
        }
        set
        {
            id_detallePedido = value;
        }
    }

    public Guid IdPedido
    {
        get
        {
            return id_pedido;
        }
        set
        {
            id_pedido = value;
        }
    }

    public int IdProducto
    {
        get
        {
            return id_producto;
        }
        set
        {
            id_producto = value;
        }
    }

    public string Nombre
    {
        get
        {
            return nombre;
        }
        set
        {
            nombre = value;
        }
    }

    public int Cantidad
    {
        get
        {
            return cantidad;
        }
        set
        {
            cantidad = value;
        }
    }

    public double Precio
    {
        get
        {
            return precio;
        }
        set
        {
            precio = value;
        }
    }

    public double Importe
    {
        get
        {
            return importe;
        }
        set
        {
            importe = value;
        }
    }
}
