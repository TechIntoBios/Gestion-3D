namespace Entidades;

public class Productos
{
    private int id_producto;
    private int id_categoria;
    private string nombre;
    private double precio;
    private bool activo; 

    public Productos()
    {
        id_producto = 0;
        id_categoria = 0;
        nombre = "Sin definir";
        precio = 0;
        activo = false; 
    }
    public Productos(int id_producto, int id_categoria, string nombre, double precio, bool activo)
    {
        this.id_producto = id_producto;
        this.id_categoria = id_categoria;
        this.nombre = nombre;
        this.precio = precio;
        this.activo = activo; 
    }
    public int Id_producto
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
    public int Id_categoria
    {
        get
        {
            return id_categoria;
        }
        set
        {
            id_categoria = value;
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

    public bool Activo
    {
        get
        {
            return activo; 
        }
        set
        {
            activo = value;
        }
    }
}