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
    private readonly NegocioJwt negocioJwt;
    private readonly NegocioProteccionLogin negocioProteccionLogin;

    public AuthController(NegocioUsuarios negocioUsuarios, NegocioJwt negocioJwt, NegocioProteccionLogin negocioProteccionLogin)
    {
        this.negocioUsuarios = negocioUsuarios;
        this.negocioJwt = negocioJwt;
        this.negocioProteccionLogin = negocioProteccionLogin;
    }

    [AllowAnonymous]
    [EnableRateLimiting(NegocioLimitesApi.Login)]
    [RequestSizeLimit(4096)]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest solicitud)
    {
        if (!NegocioProteccionLogin.EsEntradaValida(solicitud.usuario, solicitud.contrasena))
        {
            return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos." });
        }

        var nombreUsuario = solicitud.usuario!;
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocida";

        if (negocioProteccionLogin.EstaBloqueado(nombreUsuario, ip))
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, new { mensaje = "Demasiados intentos. Probá de nuevo más tarde." });
        }

        var usuario = await negocioUsuarios.Autenticar(solicitud.usuario, solicitud.contrasena);
        if (usuario is null)
        {
            negocioProteccionLogin.RegistrarFallo(nombreUsuario, ip);
            return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos." });
        }

        negocioProteccionLogin.Limpiar(nombreUsuario, ip);

        var opciones = OpcionesCookie();
        opciones.Expires = DateTimeOffset.UtcNow.AddHours(negocioJwt.DuracionHoras);
        Response.Cookies.Append(NegocioJwt.NombreCookie, negocioJwt.CrearToken(usuario), opciones);

        return Ok(new LoginResponse { Usuario = usuario.Usuario, ExpiraEnHoras = negocioJwt.DuracionHoras });
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(NegocioJwt.NombreCookie, OpcionesCookie());
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
