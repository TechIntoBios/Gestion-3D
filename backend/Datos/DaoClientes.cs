using Entidades;
using Npgsql;
using NpgsqlTypes;

namespace Datos;

public class DaoClientes
{
    // Columnas del cliente en el orden que espera leerCliente. Las usan también las consultas de pedidos.
    public const string columnas_cliente = "c.Id_cliente_Cli, c.Nombre_Cli, c.Apellido_Cli, c.CorreoElectronico_Cli, c.Zona_Cli, c.Telefono_Cli, c.Activo_Cli";

    private readonly AccesoDatos accesoDatos;
    private const string function_cliente = "SELECT fn_AgregarCliente(@NOMBRE, @APELLIDO, @CORREO, @ZONA, @TELEFONO)";
    private const string function_actualizar = "SELECT fn_ActualizarCliente(@ID_CLIENTE, @NOMBRE, @APELLIDO, @CORREO, @ZONA, @TELEFONO)";
    private const string cliente_por_telefono = "SELECT " + columnas_cliente + " FROM Clientes c WHERE c.Telefono_Cli = @TELEFONO";

    public DaoClientes(AccesoDatos datos) => accesoDatos = datos;

    public async Task<Guid> agregarCliente(Clientes clientes, NpgsqlConnection conexion, NpgsqlTransaction transaccion)
    {
        await using var command = new NpgsqlCommand(function_cliente, conexion, transaccion);
        armarParametrosAgregarCliente(command, clientes);

        var result = await command.ExecuteScalarAsync();
        if (result is not Guid idCliente)
        {
            throw new InvalidOperationException("La función no devolvió el UUID del cliente.");
        }

        return idCliente;
    }

    public async Task<Clientes?> obtenerPorTelefono(string telefono)
    {
        var clientes = await accesoDatos.consultar(
            cliente_por_telefono,
            reader => leerCliente(reader),
            parametros => parametros.Add("@TELEFONO", NpgsqlDbType.Varchar, 20).Value = telefono);

        return clientes.FirstOrDefault();
    }

    // Devuelve OK, NO_EXISTE o TELEFONO_DUPLICADO.
    public async Task<string> actualizar(Guid idCliente, Clientes clientes)
    {
        await using var connection = await accesoDatos.obtenerConexion();
        await using var command = new NpgsqlCommand(function_actualizar, connection);
        command.Parameters.Add("@ID_CLIENTE", NpgsqlDbType.Uuid).Value = idCliente;
        armarParametrosAgregarCliente(command, clientes);

        return (string)(await command.ExecuteScalarAsync())!;
    }

    public static Clientes leerCliente(NpgsqlDataReader reader, int desde = 0) => new()
    {
        Idcliente = reader.GetGuid(desde).ToString(),
        Nombre = reader.GetString(desde + 1),
        Apellido = reader.GetString(desde + 2),
        Correo = reader.IsDBNull(desde + 3) ? null! : reader.GetString(desde + 3),
        Zona = reader.GetString(desde + 4),
        Telefono = reader.GetString(desde + 5),
        Activo = reader.GetBoolean(desde + 6)
    };

    public void armarParametrosAgregarCliente(NpgsqlCommand command, Clientes clientes)
    {
        command.Parameters.Add("@NOMBRE", NpgsqlDbType.Varchar, 100).Value = clientes.Nombre;
        command.Parameters.Add("@APELLIDO", NpgsqlDbType.Varchar, 100).Value = clientes.Apellido;
        command.Parameters.Add("@CORREO", NpgsqlDbType.Varchar, 300).Value = (object?)clientes.Correo ?? DBNull.Value;
        command.Parameters.Add("@ZONA", NpgsqlDbType.Varchar, 100).Value = clientes.Zona;
        command.Parameters.Add("@TELEFONO", NpgsqlDbType.Varchar, 20).Value = clientes.Telefono;
    }
}
