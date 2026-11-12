using Entidades;
using NpgsqlTypes;

namespace Datos;

public class DaoUsuarios
{
    private readonly AccesoDatos accesoDatos;

    private const string ObtenerUsuario = "SELECT User_U, Password_U FROM Usuarios WHERE User_U = @USER AND Activo_U = TRUE";

    public DaoUsuarios(AccesoDatos datos) => accesoDatos = datos;

    public async Task<Usuarios?> ObtenerPorUsuario(string nombreUsuario)
    {
        var usuarios = await accesoDatos.consultar(
            ObtenerUsuario,
            reader => new Usuarios { Usuario = reader.GetString(0), Password = reader.GetString(1) },
            parametros => parametros.Add("@USER", NpgsqlDbType.Varchar, 30).Value = nombreUsuario);

        return usuarios.FirstOrDefault();
    }
}
