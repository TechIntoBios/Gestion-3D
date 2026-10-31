using System.Runtime.CompilerServices;
using Entidades;
using Npgsql;
using NpgsqlTypes;

namespace Datos;

public class DaoUsuarios
{
    private readonly AccesoDatos accesoDatos;

    private const string ObtenerUsuarios = "SELECT User_U, Password_U FROM Usuarios WHERE User_U = @USER AND Password_U = @PASSWORD";
    private const string ExisteUsuario = "SELECT EXISTS (SELECT 1 FROM Usuarios WHERE User_U = @USER AND Password_U = @PASSWORD)"; 
    
    public DaoUsuarios(AccesoDatos datos) => accesoDatos = datos;

    public async Task<bool>existeUsuario(Usuarios usuario)
    {
        await using var command = new NpgsqlCommand();
        armarParametrosUsuarios(command, usuario); 
        return await accesoDatos.existe(ExisteUsuario, command);
    }

    public async Task<Usuarios?> GetUsuario (Usuarios usuarios)
    {
       await using var connection = await accesoDatos.obtenerConexion();
       await using var command = new NpgsqlCommand(ObtenerUsuarios, connection);
       armarParametrosUsuarios(command, usuarios); 
       await using var reader = await command.ExecuteReaderAsync();

        if(await reader.ReadAsync())
        {
            var usuario = new Usuarios
            {
                Usuario = reader.GetString(0),
                Password = reader.GetString(1)
            };
            return usuario;
        }
        return null;
    }

    public void armarParametrosUsuarios(NpgsqlCommand command, Usuarios usuarios)
    {
        command.Parameters.Add("@USER", NpgsqlDbType.Varchar,100).Value = usuarios.Usuario;
        command.Parameters.Add("@PASSWORD", NpgsqlDbType.Varchar,100).Value = usuarios.Password;
    }
}