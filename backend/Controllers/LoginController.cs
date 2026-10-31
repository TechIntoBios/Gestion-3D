using System.Threading.Tasks;
using Entidades;
using Negocio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DTOs;


namespace Controllers;

[ApiController]
[Route("api/[controller]")]

public class LoginController : ControllerBase
{
    private readonly NegocioUsuarios negocioUsuarios; 
    public LoginController(NegocioUsuarios nUsuarios)
    {
        this.negocioUsuarios = nUsuarios;
    }

    [HttpPost("autorizacion")]
    public async Task<IActionResult> GetUsuario([FromBody] LoginRequest loginRequest)
    {
        var loginResponse = await negocioUsuarios.GetUsuario(loginRequest!.usuario!, loginRequest!.contrasena!);

        if (loginResponse is null)
        {
            return Unauthorized(new { mensaje = "Credenciales inválidas." });
        }

        return Ok();
    }
}