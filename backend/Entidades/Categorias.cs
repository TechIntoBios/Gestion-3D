namespace Entidades;

public class Categorias
{
    private int Id_Categoria;
    private string NombreCategoria;
    private bool Activo;

    public Categorias()
    {
        Id_Categoria = 0;
        NombreCategoria = "Sin definir";
        Activo = false;
    }

    public Categorias(int id_Categoria, string nombreCategoria, bool activo)
    {
        this.Id_Categoria = id_Categoria;
        this.NombreCategoria = nombreCategoria;
        this.Activo = activo;
    }

    public int IDCATEGORIA
    {
        get
        {
            return Id_Categoria;
        }
        set
        {
            Id_Categoria = value;
        }
    }

    public String NOMBRE
    {
        get
        {
            return NombreCategoria;
        }
        set
        {
            NombreCategoria = value;
        }
    }

    public bool ACTIVO
    {
        get
        {
            return Activo;
        }
        set
        {
            Activo =  value;
        }
    }

}