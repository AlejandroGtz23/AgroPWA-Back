using Agro.Application.Sincronizacion.Commands.ActualizarPreferencias;
using Agro.Application.Sincronizacion.Commands.ProcesarSincronizacion;
using Agro.Application.Sincronizacion.Commands.RegistrarDispositivo;
using Agro.Application.Sincronizacion.Common;
using Agro.Application.Sincronizacion.Queries.ObtenerEstado;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace Agro.Api.Controllers;

[ApiController]
[Authorize]
[Route("api")]
[Produces("application/json")]
public class SincronizacionController(
    RegistrarDispositivoHandler registrarDispositivo,
    ActualizarPreferenciasHandler actualizarPreferencias,
    ObtenerEstadoHandler obtenerEstado,
    ProcesarSincronizacionHandler procesarSincronizacion) : ControllerBase
{
    private int IdUsuario => int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    [HttpPost("dispositivos")]
    [ProducesResponseType<EstadoSincronizacionDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<EstadoSincronizacionDto>(StatusCodes.Status200OK)]      // ya estaba registrado
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EstadoSincronizacionDto>> RegistrarDispositivo(RegistrarDispositivoCommand comando,
        CancellationToken ct)
    {
        var (dispositivo, creado) = await registrarDispositivo.EjecutarAsync(IdUsuario, comando, ct);
        return creado ? StatusCode(StatusCodes.Status201Created, dispositivo) : Ok(dispositivo);
    }

    [HttpPut("dispositivos/{id:int}/preferencias")]
    [ProducesResponseType<EstadoSincronizacionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EstadoSincronizacionDto>> ActualizarPreferencias(int id,
        ActualizarPreferenciasCommand comando, CancellationToken ct) =>
        Ok(await actualizarPreferencias.EjecutarAsync(id, IdUsuario, comando, ct));

    [HttpGet("sincronizacion/estado")]
    [ProducesResponseType<EstadoSincronizacionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EstadoSincronizacionDto>> ObtenerEstado([FromQuery] int idDispositivo,
        CancellationToken ct) =>
        Ok(await obtenerEstado.EjecutarAsync(idDispositivo, IdUsuario, ct));

    [HttpPost("sincronizacion")]
    [ProducesResponseType<ResultadoSincronizacionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResultadoSincronizacionDto>> Sincronizar(ProcesarSincronizacionCommand comando,
    CancellationToken ct) =>
    Ok(await procesarSincronizacion.EjecutarAsync(IdUsuario, comando, ct));
}