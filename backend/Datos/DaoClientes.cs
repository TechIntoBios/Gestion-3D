using Entidades;
using Npgsql;
using NpgsqlTypes;

namespace Datos;

public class DaoClientes
{
    private readonly AccesoDatos accesoDatos;
    private const string procedimiento_cliente = "sp_AgregarCliente";
    private const string existe_cliente = "SELECT EXISTS (SELECT 1 FROM Clientes WHERE Telefono_Cli = @TELEFONO)";
    private const string function_cliente = "SELECT fn_AgregarCliente(@NOMBRE, @APELLIDO, @CORREO, @ZONA, @TELEFONO)";
    private const string cliente_por_telefono = "SELECT Id_cliente_Cli, Nombre_Cli, Apellido_Cli, CorreoElectronico_Cli, Zona_Cli, Telefono_Cli, Activo_Cli FROM Clientes WHERE Telefono_Cli = @TELEFONO";

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

    public async Task<Clientes?> obtenerPorTelefono(String telefono)
    {
        await using var connection = await accesoDatos.obtenerConexion();
        await using var command = new NpgsqlCommand(cliente_por_telefono, connection);
        command.Parameters.Add("@TELEFONO", NpgsqlDbType.Varchar, 20).Value = telefono;
        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new Clientes
        {
            Idcliente = reader.GetGuid(0).ToString(),
            Nombre = reader.GetString(1),
            Apellido = reader.GetString(2),
            Correo = reader.IsDBNull(3) ? null! : reader.GetString(3),
            Zona = reader.GetString(4),
            Telefono = reader.GetString(5),
            Activo = reader.GetBoolean(6)
        };
    }

    public void armarParametrosAgregarCliente(NpgsqlCommand command, Clientes clientes)
    {
        command.Parameters.Add("@NOMBRE", NpgsqlDbType.Varchar, 100).Value = clientes.Nombre;
        command.Parameters.Add("@APELLIDO", NpgsqlDbType.Varchar, 100).Value = clientes.Apellido;
        command.Parameters.Add("@CORREO", NpgsqlDbType.Varchar, 300).Value = (object?)clientes.Correo ?? DBNull.Value;  
        command.Parameters.Add("@ZONA", NpgsqlDbType.Varchar, 100).Value = clientes.Zona; 
        command.Parameters.Add("@TELEFONO", NpgsqlDbType.Varchar, 20).Value = clientes.Telefono;
    }
}