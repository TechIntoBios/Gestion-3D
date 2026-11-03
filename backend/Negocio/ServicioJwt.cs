using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Entidades;
using Microsoft.IdentityModel.Tokens;

namespace Negocio;

// Unica fuente de configuracion JWT: emite los tokens y define como se validan.
public sealed class ServicioJwt
{
    public const string NombreCookie = "__Host-zunox_sesion";
    private const int BytesMinimosClave = 32;
    private const int DuracionPorDefectoHoras = 8;

    private readonly SymmetricSecurityKey clave;
    private readonly string emisor;
    private readonly string audiencia;

    public int DuracionHoras { get; }

    public ServicioJwt(IConfiguration configuracion)
    {
        var secreto = configuracion["JwtSettings:SecretKey"];
        if (string.IsNullOrWhiteSpace(secreto))
        {
            throw new InvalidOperationException("Falta configurar 'JwtSettings:SecretKey'.");
        }

        var bytesClave = Encoding.UTF8.GetBytes(secreto);
        if (bytesClave.Length < BytesMinimosClave)
        {
            throw new InvalidOperationException($"'JwtSettings:SecretKey' debe tener al menos {BytesMinimosClave} bytes.");
        }

        clave = new SymmetricSecurityKey(bytesClave);
        emisor = configuracion["JwtSettings:Issuer"] ?? "Sistema3DBackend";
        audiencia = configuracion["JwtSettings:Audience"] ?? "Sistema3DFrontend";
        DuracionHoras = int.TryParse(configuracion["JwtSettings:DuracionHoras"], out var horas) && horas > 0
            ? horas
            : DuracionPorDefectoHoras;
    }

    public TokenValidationParameters ParametrosValidacion() => new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = emisor,
        ValidAudience = audiencia,
        IssuerSigningKey = clave,
        ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
        NameClaimType = JwtRegisteredClaimNames.UniqueName,
        ClockSkew = TimeSpan.Zero
    };

    public string CrearToken(Usuarios usuario)
    {
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.UniqueName, usuario.Usuario),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ]),
            Expires = DateTime.UtcNow.AddHours(DuracionHoras),
            Issuer = emisor,
            Audience = audiencia,
            SigningCredentials = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256)
        };

        var manejador = new JwtSecurityTokenHandler();
        return manejador.WriteToken(manejador.CreateToken(descriptor));
    }
}
