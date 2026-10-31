using Entidades;
using Npgsql;
using NpgsqlTypes;

namespace Datos;

public class DaoCategorias
{
    private readonly AccesoDatos accesoDatos;

    private const string consulta = "SELECT Id_Categoria_Cat, Nombre_Cat, Activo_Cat FROM Categorias";

    private const string procedimiento_categorias = "sp_AgregarCategoria";
    private const string ExisteCategorias = "SELECT EXISTS (SELECT 1 FROM Categorias WHERE Nombre_Cat = @NOMBRE)";

    public DaoCategorias(AccesoDatos datos) => accesoDatos = datos;

    public async Task<bool>AgregarCategorias(Categorias categorias)
    {
        var command = new NpgsqlCommand();
        armarParametrosAgregarCategorias(command, categorias);
        return await accesoDatos.ejecutarProcedimientoAlmacenado(command,procedimiento_categorias) != 0;
    }

    public async Task<bool>ExisteCategoria(Categorias categorias) 
    {
        var command = new NpgsqlCommand();
        armarParametrosAgregarCategorias(command, categorias); 
        return await accesoDatos.existe(ExisteCategorias, command);
    }

    public void armarParametrosAgregarCategorias(NpgsqlCommand command, Categorias categorias)
    {
        command.Parameters.Add("@NOMBRE",NpgsqlDbType.Varchar,100).Value = categorias.NOMBRE;
    }

    public async Task<List<Categorias>> ListarTodasLasCategorias()
    {
        var Lista = new List<Categorias>();
        await using var connection = await accesoDatos.obtenerConexion(); 
        await using var command = new NpgsqlCommand(consulta, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var Categoria = new Categorias();

            Categoria.IDCATEGORIA = reader.GetInt32(0);
            Categoria.NOMBRE = reader.IsDBNull(1) ? "No contiene" : reader.GetString(1);
            Categoria.ACTIVO = reader.GetBoolean(2); 
            Lista.Add(Categoria);
        }

        return Lista;
    }


}