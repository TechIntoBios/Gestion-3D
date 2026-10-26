using Entidades;
using Npgsql;
using NpgsqlTypes;

namespace Datos;

public class DaoUsuarios
{
    private readonly AccesoDatos accesoDatos;

    private const string ExisteUsuario = "SELECT EXISTS (SELECT 1 FROM usuarios WHERE usuario = @usuario AND contrasena = @contrasena)"; 
    
    public DaoUsuarios(AccesoDatos datos) => accesoDatos = datos;

    public async Task<bool>existeUsuario(Usuarios usuario)
    {
        var command = new NpgsqlCommand();
        command.Parameters.Add("@usuario", NpgsqlDbType.Varchar,100).Value = usuario.Usuario;
        command.Parameters.Add("@contrasena", NpgsqlDbType.Varchar,100).Value = usuario.Password;
        return await accesoDatos.existe("", command);
    }
}