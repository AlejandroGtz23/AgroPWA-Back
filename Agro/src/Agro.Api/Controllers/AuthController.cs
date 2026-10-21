using Agro.Application.Auth.Commands.RegistrarUsuario;
using Microsoft.AspNetCore.Mvc;

namespace Agro.Api.Controllers;
/// <summary>
/// Registro, Inicio, y cierre de sesion
/// </summary>


[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController(RegistrarUsuarioHandler registarUsuario) : ControllerBase
{
    [HttpPost("registro")]
    [ProducesResponseType<UsuarioRegistradoDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]

    public async Task<ActionResult<UsuarioRegistradoDto>> Registar(RegistrarUsuarioCommand comando, CancellationToken ct)
    {
        var usuario = await registarUsuario.EjecutarAsync(comando, ct);
        return StatusCode(StatusCodes.Status201Created, usuario);
    }
}
