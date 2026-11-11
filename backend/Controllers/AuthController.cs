using DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Negocio;

namespace Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly NegocioUsuarios negocioUsuarios;
    private readonly ServicioJwt servicioJwt;
    private readonly ProteccionLogin proteccionLogin;

    public AuthController(NegocioUsuarios negocioUsuarios, ServicioJwt servicioJwt, ProteccionLogin proteccionLogin)
    {
        this.negocioUsuarios = negocioUsuarios;
        this.servicioJwt = servicioJwt;
        this.proteccionLogin = proteccionLogin;
    }

    [AllowAnonymous]
    [EnableRateLimiting(LimitesApi.Login)]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest solicitud)
    {
        var nombreUsuario = solicitud.usuario ?? string.Empty;

        if (proteccionLogin.EstaBloqueado(nombreUsuario))
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, new { mensaje = "Demasiados intentos. Probá de nuevo más tarde." });
        }

        var usuario = await negocioUsuarios.Autenticar(solicitud.usuario, solicitud.contrasena);
        if (usuario is null)
        {
            proteccionLogin.RegistrarFallo(nombreUsuario);
            return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos." });
        }

        proteccionLogin.Limpiar(nombreUsuario);

        var opciones = OpcionesCookie();
        opciones.Expires = DateTimeOffset.UtcNow.AddHours(servicioJwt.DuracionHoras);
        Response.Cookies.Append(ServicioJwt.NombreCookie, servicioJwt.CrearToken(usuario), opciones);

        return Ok(new LoginResponse { Usuario = usuario.Usuario, ExpiraEnHoras = servicioJwt.DuracionHoras });
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(ServicioJwt.NombreCookie, OpcionesCookie());
        return NoContent();
    }

    [HttpGet("sesion")]
    public IActionResult Sesion() => Ok(new { usuario = User.Identity?.Name });

    private static CookieOptions OpcionesCookie() => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Path = "/"
    };
}
