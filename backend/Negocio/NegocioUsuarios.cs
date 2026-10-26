using Datos;
using Entidades;

namespace Negocio;

public class NegocioUsuarios
{
    private readonly DaoUsuarios daoUsuarios;

    public NegocioUsuarios(DaoUsuarios dao) => daoUsuarios = dao;

    public async Task<bool> existeUsuario(string nombreUsuario, string password)
    {
        var usuario = new Usuarios();
        usuario.Usuario = nombreUsuario;
        usuario.Password = password;
        
        return await daoUsuarios.existeUsuario(usuario);
    }
}