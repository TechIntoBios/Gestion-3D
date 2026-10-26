
namespace Entidades; 

public class Pedidos
{
    private Guid id_pedido; 
    private Guid id_cliente; 
    private int id_estado_pedido; 
    private int numero; 
    private String nota; 
    private String seguimiento; 
    private DateTimeOffset fecha; 

    public Pedidos()
    {
        id_pedido = Guid.Empty; 
        id_cliente = Guid.Empty; 
        id_estado_pedido = 0; 
        numero = 0; 
        nota = "Sin definir"; 
        seguimiento = "Sin definir"; 
        fecha = DateTimeOffset.Now; 
    } 

    public Pedidos(Guid id_pedido, Guid id_cliente, int id_estado_pedido, int numero, String nota, String seguimiento, DateTimeOffset fecha)
    {
        this.id_pedido = id_pedido; 
        this.id_cliente = id_cliente; 
        this.id_estado_pedido = id_estado_pedido;
        this.numero = numero; 
        this.nota = nota; 
        this.seguimiento = seguimiento; 
        this.fecha = fecha; 
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

    public Guid IdCliente
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

    public int IdEstadoPedido
    {
        get
        {
            return id_estado_pedido;
        }
        set
        {
            id_estado_pedido = value;
        }
    }

    public int Numero
    {
        get
        {
            return numero;
        }
        set
        {
            numero = value;
        }
    }

    public String Nota
    {
        get
        {
            return nota;
        }
        set
        {
            nota = value;
        }
    }

    public String Seguimiento
    {
        get
        {
            return seguimiento;
        }
        set
        {
            seguimiento = value;
        }
    }

    public DateTimeOffset Fecha
    {
        get
        {
            return fecha;
        }
        set
        {
            fecha = value;
        }
    }
}