using System.IdentityModel.Tokens.Jwt;
using System.Text;
using BCrypt.Net;
using Datos;
using DTOs;
using Entidades;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
namespace Negocio;

public class NegocioUsuarios
{
    private readonly DaoUsuarios daoUsuarios;
    private readonly IConfiguration configuracion;

    public NegocioUsuarios(DaoUsuarios dao, IConfiguration configuracion)
    {
        this.daoUsuarios = dao;
        this.configuracion = configuracion;
    }
    public async Task<bool> existeUsuario(string nombreUsuario, string password)
    {
        var usuario = new Usuarios();
        usuario.Usuario = nombreUsuario;
        usuario.Password = password;
        
        return await daoUsuarios.existeUsuario(usuario);
    }

    public async Task<LoginResponse?> GetUsuario(string nombreUsuario, string Password)
    {
        var usuarios = new Usuarios(); 
        usuarios.Usuario = nombreUsuario; 
        usuarios.Password = Password; 
        // Llamar al método GetUsuario del DaoUsuarios para obtener el usuario desde la base de datos
        var usuarioExistente = await daoUsuarios.GetUsuario(usuarios);

        if (usuarioExistente is null)
        {
            // El usuario no existe en la base de datos
            return null;
        }

        // Verificar password

        bool passwordCorrecto = BCrypt.Net.BCrypt.Verify(usuarios.Password, usuarioExistente.Password);
        if (!passwordCorrecto)
        {
            // La contraseña es incorrecta
            return null;
        }

        int duracionHoras = int.TryParse(configuracion["Jwt:DuracionHoras"], out var horas) && horas > 0 ? horas : 10;
        string token = FabricarTokenJwt(usuarioExistente, duracionHoras);
        return new LoginResponse { Token = token, Username = usuarioExistente.Usuario, ExpiraEnHoras = duracionHoras };
    }

    private string FabricarTokenJwt(Usuarios usuario, int duracionHoras)
    {
        var secretKey = configuracion["JwtSettings:SecretKey"]
            ?? throw new InvalidOperationException("Falta configurar 'JwtSettings:SecretKey'.");

        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var symmetricKey = new SymmetricSecurityKey(keyBytes);
        var signingCredentials = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.UniqueName, usuario.Usuario),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(duracionHoras),
            Issuer = configuracion["JwtSettings:Issuer"] ?? "Sistema3DBackend",
            Audience = configuracion["JwtSettings:Audience"] ?? "Sistema3DFrontend",
            SigningCredentials = signingCredentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
    }
}