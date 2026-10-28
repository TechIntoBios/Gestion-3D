using System.Runtime.CompilerServices;
using Entidades;
using Npgsql;
using NpgsqlTypes;

namespace Datos;

public class DaoUsuarios
{
    private readonly AccesoDatos accesoDatos;

    private const string ObtenerUsuarios = "SELECT usuario, contrasena FROM usuarios WHERE usuario = @usuario AND contrasena = @contrasena";
    private const string ExisteUsuario = "SELECT EXISTS (SELECT 1 FROM usuarios WHERE usuario = @usuario AND contrasena = @contrasena)"; 
    
    public DaoUsuarios(AccesoDatos datos) => accesoDatos = datos;

    public async Task<bool>existeUsuario(Usuarios usuario)
    {
        var command = new NpgsqlCommand();
        command.Parameters.Add("@usuario", NpgsqlDbType.Varchar,100).Value = usuario.Usuario;
        command.Parameters.Add("@contrasena", NpgsqlDbType.Varchar,100).Value = usuario.Password;
        return await accesoDatos.existe(ExisteUsuario, command);
    }

    public async Task<Usuarios?> GetUsuario (Usuarios usuarios)
    {
       var connection = await accesoDatos.obtenerConexion();
       var command = new NpgsqlCommand(ObtenerUsuarios, connection);
       command.Parameters.AddWithValue("@usuario", usuarios.Usuario.Trim());
       command.Parameters.AddWithValue("@contrasena", usuarios.Password);
       using var reader = await command.ExecuteReaderAsync();

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
}