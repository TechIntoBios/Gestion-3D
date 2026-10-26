

namespace Entidades; 

public class Clientes
{
    private String id_cliente; 
	private String nombre;
	private String apellido;
	private String correo;
	private String zona;
	private String telefono;
	private bool activo;

    public Clientes()
    {
        id_cliente = "Sin definir";
        nombre = "Sin definir";
        apellido = "Sin definir";
        correo = "Sin definir"; 
        zona = "Sin definir";
        telefono = "Sin definir";
        activo = false;
    }

    public Clientes(String id_cliente, String nombre, String apellido, String correo, String zona, String telefono, bool activo)
    {
        this.id_cliente = id_cliente; 
        this.nombre = nombre; 
        this.apellido = apellido; 
        this.correo = correo; 
        this.zona = zona; 
        this.telefono = telefono; 
        this.activo = activo;
    }

    public String Idcliente
    {
        get
        {
            return id_cliente; 
        }
        set
        {
            id_cliente = value; 
        }
    }

    public String Nombre
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

    public String Apellido
    {
        get
        {
            return apellido;
        }
        set
        {
            apellido = value;
        }
    }

    public String Correo
    {
        get
        {
            return correo;
        }
        set
        {
            correo= value;
        }
    }

    public String Zona
    {
        get
        {
            return zona; 
        }
        set
        {
            zona = value; 
        }
    }

    public String Telefono
    {
        get
        {
            return telefono;
        }
        set
        {
            telefono = value;
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