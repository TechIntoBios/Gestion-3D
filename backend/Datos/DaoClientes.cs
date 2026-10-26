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

    public DaoClientes(AccesoDatos datos) => accesoDatos = datos;

    // public async Task<String> agregarCliente(Clientes clientes)
    // {
    //     var command = new NpgsqlCommand();
    //     armarParametrosAgregarCliente(command, clientes);
    //     object? result = await accesoDatos.ejecutarFunction(command, function_cliente); 
    //     if(result == null || result == DBNull.Value)
    //     {
    //         throw new Exception("No se pudo registrar el cliente.");
    //     }
    //     return (String)result;
    //     //return await accesoDatos.ejecutarProcedimientoAlmacenado(command, procedimiento_cliente) != 0;
    // }

    // public async Task<bool> existeCliente(Clientes clientes)
    // {
    //     await using var command = new NpgsqlCommand();
    //     command.Parameters.Add("@TELEFONO", NpgsqlDbType.Varchar, 20).Value = clientes.Telefono;
    //     return await accesoDatos.existe(existe_cliente, command);
    // }


    public async Task<bool> existeCliente(Clientes clientes, NpgsqlConnection conexion, NpgsqlTransaction transaccion)
    {
        await using var command = new NpgsqlCommand(existe_cliente, conexion, transaccion);
        command.Parameters.Add("@TELEFONO", NpgsqlDbType.Varchar, 20).Value = clientes.Telefono;
        return (bool)(await command.ExecuteScalarAsync())!;
    }
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

    public void armarParametrosAgregarCliente(NpgsqlCommand command, Clientes clientes)
    {
        command.Parameters.Add("@NOMBRE", NpgsqlDbType.Varchar, 100).Value = clientes.Nombre;
        command.Parameters.Add("@APELLIDO", NpgsqlDbType.Varchar, 100).Value = clientes.Apellido;
        command.Parameters.Add("@CORREO", NpgsqlDbType.Varchar, 300).Value = (object?)clientes.Correo ?? DBNull.Value;  
        command.Parameters.Add("@ZONA", NpgsqlDbType.Varchar, 100).Value = clientes.Zona; 
        command.Parameters.Add("@TELEFONO", NpgsqlDbType.Varchar, 20).Value = clientes.Telefono;
    }
}