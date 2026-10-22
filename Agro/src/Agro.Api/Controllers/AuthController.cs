using Agro.Application.Auth.Commands.IniciarSesion;
using Agro.Application.Auth.Commands.RegistrarUsuario;
using Agro.Application.Auth.Common;
using Microsoft.AspNetCore.Mvc;

namespace Agro.Api.Controllers;
/// <summary>
/// Registro, Inicio, y cierre de sesion
/// </summary>


[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController(RegistrarUsuarioHandler registrarUsuario, IniciarSesionHandler iniciarSesion) : ControllerBase
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

    private (string? Ip, string? Agente) DatosCliente()
    {
        var agente = Request.Headers.UserAgent.ToString();
        return (HttpContext.Connection.RemoteIpAddress?.ToString(),
                agente.Length == 0 ? null : agente[..Math.Min(agente.Length, 300)]);   // columna VARCHAR(300)
    }
}
