namespace Entidades;

public class EstadoPedido
{
    private int id_estadoPedido;
    private string codigo;
    private string estado;

    public EstadoPedido()
    {
        id_estadoPedido = 0;
        codigo = "Sin definir";
        estado = "Sin definir";
    }

    public EstadoPedido(int id_estadoPedido, string codigo, string estado)
    {
        this.id_estadoPedido = id_estadoPedido;
        this.codigo = codigo;
        this.estado = estado;
    }

    public int IdEstadoPedido
    {
        get
        {
            return id_estadoPedido;
        }
        set
        {
            id_estadoPedido = value;
        }
    }

    public string Codigo
    {
        get
        {
            return codigo;
        }
        set
        {
            codigo = value;
        }
    }

    public string Estado
    {
        get
        {
            return estado;
        }
        set
        {
            estado = value;
        }
    }
}
