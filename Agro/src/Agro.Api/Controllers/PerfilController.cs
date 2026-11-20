using Agro.Application.Perfil.Commands.ActualizarPerfil;
using Agro.Application.Perfil.Commands.CambiarContrasena;
using Agro.Application.Perfil.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using Agro.Application.Perfil.Queries.ObtenerPerfil;

namespace Agro.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/perfil")]
[Produces("application/json")]
public class PerfilController(
    ObtenerPerfilHandler obtenerPerfil,
    ActualizarPerfilHandler actualizarPerfil,
    CambiarContrasenaHandler cambiarContrasena) : ControllerBase
{
    private int IdUsuario => int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    [HttpGet]
    [ProducesResponseType<PerfilDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PerfilDto>> Obtener(CancellationToken ct) =>
        Ok(await obtenerPerfil.EjecutarAsync(IdUsuario, ct));

    [HttpPut]
    [ProducesResponseType<PerfilDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PerfilDto>> Actualizar(ActualizarPerfilCommand comando, CancellationToken ct) =>
        Ok(await actualizarPerfil.EjecutarAsync(IdUsuario, comando, ct));

    [HttpPut("contrasena")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CambiarContrasena(CambiarContrasenaCommand comando, CancellationToken ct)
    {
        var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti)!;   // la sesión actual sigue abierta
        await cambiarContrasena.EjecutarAsync(IdUsuario, jti, comando, ct);
        return NoContent();
    }
}