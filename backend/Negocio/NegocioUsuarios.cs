using System.Text;
using BCrypt.Net;
using Datos;
using Entidades;

namespace Negocio;

public class NegocioUsuarios
{
    private const int LongitudMaximaUsuario = 30;
    private const int BytesMaximosContrasena = 72; 

    private static readonly Lazy<string> HashFicticio = new(() => BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()));

    private readonly DaoUsuarios daoUsuarios;
    private readonly ILogger<NegocioUsuarios> logger;

    public NegocioUsuarios(DaoUsuarios dao, ILogger<NegocioUsuarios> logger)
    {
        this.daoUsuarios = dao;
        this.logger = logger;
    }

    public async Task<Usuarios?> Autenticar(string? nombreUsuario, string? contrasena)
    {
        if (string.IsNullOrWhiteSpace(nombreUsuario) || nombreUsuario.Length > LongitudMaximaUsuario || string.IsNullOrEmpty(contrasena) || Encoding.UTF8.GetByteCount(contrasena) > BytesMaximosContrasena)
        {
            return null;
        }

        var usuario = await daoUsuarios.ObtenerPorUsuario(nombreUsuario);
        var contrasenaCorrecta = VerificarContrasena(contrasena, usuario?.Password ?? HashFicticio.Value);

        return usuario is not null && contrasenaCorrecta ? usuario : null;
    }

    private bool VerificarContrasena(string contrasena, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(contrasena, hash);
        }
        catch (SaltParseException)
        {
            logger.LogWarning("Hay un usuario cuya contraseña guardada no es un hash BCrypt válido.");
            return false;
        }
    }
}
