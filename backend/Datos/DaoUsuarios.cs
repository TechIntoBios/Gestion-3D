using Entidades;
using Npgsql;
using NpgsqlTypes;

namespace Datos;

public class DaoUsuarios
{
    private readonly AccesoDatos accesoDatos;

    private const string ObtenerUsuario = "SELECT User_U, Password_U FROM Usuarios WHERE User_U = @USER AND Activo_U = TRUE";

    public DaoUsuarios(AccesoDatos datos) => accesoDatos = datos;

    public async Task<Usuarios?> ObtenerPorUsuario(string nombreUsuario)
    {
        await using var connection = await accesoDatos.obtenerConexion();
        await using var command = new NpgsqlCommand(ObtenerUsuario, connection);
        command.Parameters.Add("@USER", NpgsqlDbType.Varchar, 30).Value = nombreUsuario;
        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new Usuarios
            {
                Usuario = reader.GetString(0),
                Password = reader.GetString(1)
            };
        }

        return null;
    }

        public void armarParametrosUsuarios(NpgsqlCommand command, Usuarios usuarios)
    {
        command.Parameters.Add("@USER", NpgsqlDbType.Varchar,100).Value = usuarios.Usuario;
        command.Parameters.Add("@PASSWORD", NpgsqlDbType.Varchar,100).Value = usuarios.Password;
    }
}
