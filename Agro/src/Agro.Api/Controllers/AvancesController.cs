using Agro.Application.Avances.Commands.RegistrarAvance;
using Agro.Application.Avances.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace Agro.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/cultivos/{idCultivo:int}/avances")]
[Produces("application/json")]
public class AvancesController(RegistrarAvanceHandler registrarAvance) : ControllerBase
{
    private int IdUsuario => int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    [HttpPost]
    [ProducesResponseType<AvanceDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<AvanceDto>(StatusCodes.Status200OK)]          // ya existía (mismo idLocal)
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AvanceDto>> Registrar(int idCultivo, RegistrarAvanceCommand comando, CancellationToken ct)
    {
        var (avance, creado) = await registrarAvance.EjecutarAsync(idCultivo, IdUsuario, comando, ct);
        return creado ? StatusCode(StatusCodes.Status201Created, avance) : Ok(avance);
    }
}