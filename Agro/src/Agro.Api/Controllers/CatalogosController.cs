using Agro.Application.Catalogos.Queries.ObtenerTiposCultivo;
using Agro.Application.Cultivos.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Agro.Api.Controller;

[ApiController]
[Authorize]
[Route("api/catalogos")]
[Produces("application/json")]
public class CatalogosController(ObtenerTiposCultivoHandler obtenerTiposCultivo) : ControllerBase
{
    [HttpGet("tipos-cultivo")]
    [ProducesResponseType<IReadOnlyList<TipoCultivoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<TipoCultivoDto>>> ObtenerTiposCultivo(CancellationToken ct) =>
        Ok(await obtenerTiposCultivo.EjecutarAsync(ct));
}