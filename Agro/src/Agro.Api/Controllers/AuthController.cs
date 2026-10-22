using Agro.Application.Auth.Commands.CerrarSesion;
using Agro.Application.Auth.Commands.IniciarSesion;
using Agro.Application.Auth.Commands.RegistrarUsuario;
using Agro.Application.Auth.Commands.RenovarSesion;
using Agro.Application.Auth.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace Agro.Api.Controllers;
/// <summary>
/// Registro, Inicio, y cierre de sesion
/// </summary>


[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController(
    RegistrarUsuarioHandler registrarUsuario,
    IniciarSesionHandler iniciarSesion,
    RenovarSesionHandler renovarSesion,
    CerrarSesionHandler cerrarSesion) : ControllerBase
{
    [HttpPost("registro")]
    [ProducesResponseType<SesionDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SesionDto>> Registrar(RegistrarUsuarioCommand comando, CancellationToken ct)
    {
        var (ip, agente) = DatosCliente();
        var sesion = await registrarUsuario.EjecutarAsync(comando, ip, agente, ct);
        return StatusCode(StatusCodes.Status201Created, sesion);
    }

    [EnableRateLimiting("login")]
    [HttpPost("login")]
    [ProducesResponseType<SesionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SesionDto>> IniciarSesion(IniciarSesionCommand comando, CancellationToken ct)
    {
        var (ip, agente) = DatosCliente();
        return Ok(await iniciarSesion.EjecutarAsync(comando, ip, agente, ct));
    }

    /// <summary>Entrega tokens nuevos con el refresh token. 200 tokens · 401 sesión vencida, cerrada o refresh token ya usado.</summary>
    [HttpPost("renovar")]
    [ProducesResponseType<SesionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SesionDto>> Renovar(RenovarSesionCommand comando, CancellationToken ct) =>
    Ok(await renovarSesion.EjecutarAsync(comando, ct));

    /// <summary>Datos del usuario de la sesión actual (ruta protegida de prueba). 200 · 401 sin token o sesión cerrada.</summary>
    [Authorize]
    [HttpGet("yo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Yo() => Ok(new
    {
        id = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!),
        correo = User.FindFirstValue(JwtRegisteredClaimNames.Email),
        rol = User.FindFirstValue("role")
    });

    private (string? Ip, string? Agente) DatosCliente()
    {
        var agente = Request.Headers.UserAgent.ToString();
        return (HttpContext.Connection.RemoteIpAddress?.ToString(),
                agente.Length == 0 ? null : agente[..Math.Min(agente.Length, 300)]);   // columna VARCHAR(300)
    }


    /// <summary>Cierra la sesión actual. 204 cerrada · 401 sin token o sesión ya cerrada.</summary>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CerrarSesion(CancellationToken ct)
    {
        var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti)!;   // del token ya validado
        await cerrarSesion.EjecutarAsync(new CerrarSesionCommand(jti), ct);
        return NoContent();
    }
}
