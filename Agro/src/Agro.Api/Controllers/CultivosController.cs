using Agro.Application.Common.Models;
using Agro.Application.Cultivos.Commands.RegistrarCultivo;
using Agro.Application.Cultivos.Common;
using Agro.Application.Cultivos.Queries.ObtenerDetalleCultivo;
using Agro.Application.Cultivos.Queries.ObtenerMisCultivos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace Agro.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/cultivos")]
[Produces("application/json")]
public class CultivosController(RegistrarCultivoHandler registrarCultivo, 
    ObtenerMisCultivosHandler obtenerMisCultivos,
    ObtenerDetalleCultivoHandler obtenerDetalle) : ControllerBase
{
    private int IdUsuario => int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    [HttpPost]
    [ProducesResponseType<CultivoDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<CultivoDto>(StatusCodes.Status200OK)]          // ya existía (mismo idLocal)
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CultivoDto>> Registrar(RegistrarCultivoCommand comando, CancellationToken ct)
    {
        var (cultivo, creado) = await registrarCultivo.EjecutarAsync(IdUsuario, comando, ct);
        return creado ? StatusCode(StatusCodes.Status201Created, cultivo) : Ok(cultivo);
    }

    [HttpGet]
    [ProducesResponseType<Pagina<CultivoResumenDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Pagina<CultivoResumenDto>>> ObtenerMisCultivos(
    [FromQuery] string? buscar, [FromQuery] string? estado, [FromQuery] int? idTipoCultivo,
    CancellationToken ct, [FromQuery] int pagina = 1, [FromQuery] int tamano = 20) =>
    Ok(await obtenerMisCultivos.EjecutarAsync(IdUsuario,
        new ObtenerMisCultivosQuery(buscar, estado, idTipoCultivo, pagina, tamano), ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType<DetalleCultivoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<DetalleCultivoDto>> ObtenerDetalle(int id, CancellationToken ct) =>
    Ok(await obtenerDetalle.EjecutarAsync(id, IdUsuario, ct));
}
