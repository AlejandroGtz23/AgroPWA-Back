using Agro.Application.Dashboard.Common;
using Agro.Application.Dashboard.Queries.ObtenerResumenDashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace Agro.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
[Produces("application/json")]
public class DashboardController(ObtenerResumenDashboardHandler obtenerResumen) : ControllerBase
{
    [HttpGet("resumen")]
    [ProducesResponseType<ResumenDashboardDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ResumenDashboardDto>> ObtenerResumen(CancellationToken ct) =>
        Ok(await obtenerResumen.EjecutarAsync(
            int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!), ct));
}