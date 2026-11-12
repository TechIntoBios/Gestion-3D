using Entidades;
using Npgsql;
using NpgsqlTypes;

namespace Datos;

public class DaoCategorias
{
    private readonly AccesoDatos accesoDatos;

    private const string consulta = "SELECT Id_Categoria_Cat, Nombre_Cat, Activo_Cat FROM Categorias";
    private const string consulta_activas = consulta + " WHERE Activo_Cat = TRUE";
    private const string procedimiento_categorias = "sp_AgregarCategoria";
    private const string function_actualizar = "SELECT fn_ActualizarCategoria(@ID_CATEGORIA, @NOMBRE, @ACTIVO)";
    private const string ExisteCategorias = "SELECT EXISTS (SELECT 1 FROM Categorias WHERE LOWER(Nombre_Cat) = LOWER(@nombre))";

    public DaoCategorias(AccesoDatos datos) => accesoDatos = datos;

    public async Task AgregarCategorias(Categorias categorias)
    {
        var command = new NpgsqlCommand();
        armarParametrosAgregarCategorias(command, categorias);
        await accesoDatos.ejecutarProcedimientoAlmacenado(procedimiento_categorias, command);
    }

    public async Task<string> actualizar(Categorias categorias)
    {
        await using var connection = await accesoDatos.obtenerConexion();
        await using var command = new NpgsqlCommand(function_actualizar, connection);
        command.Parameters.Add("@ID_CATEGORIA", NpgsqlDbType.Integer).Value = categorias.IDCATEGORIA;
        command.Parameters.Add("@NOMBRE", NpgsqlDbType.Varchar, 100).Value = categorias.NOMBRE;
        command.Parameters.Add("@ACTIVO", NpgsqlDbType.Boolean).Value = categorias.ACTIVO;

        return (string)(await command.ExecuteScalarAsync())!;
    }

    public async Task<bool> ExisteCategoria(Categorias categorias)
    {
        var command = new NpgsqlCommand();
        armarParametrosAgregarCategorias(command, categorias);
        return await accesoDatos.existe(ExisteCategorias, command);
    }

    public void armarParametrosAgregarCategorias(NpgsqlCommand command, Categorias categorias)
    {
        command.Parameters.Add("nombre", NpgsqlDbType.Varchar, 100).Value = categorias.NOMBRE;
    }

    public Task<List<Categorias>> ListarTodasLasCategorias(bool soloActivas) =>
        accesoDatos.consultar(soloActivas ? consulta_activas : consulta, reader => new Categorias
        {
            IDCATEGORIA = reader.GetInt32(0),
            NOMBRE = reader.IsDBNull(1) ? "No contiene" : reader.GetString(1),
            ACTIVO = reader.GetBoolean(2)
        });
}
