using Agro.Application.Avances.Common;
using Agro.Application.Catalogos.Queries.ObtenerTiposCultivo;
using Agro.Application.Cultivos.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Agro.Application.Catalogos.Queries.ObtenerTiposActividad;

namespace Agro.Api.Controller;

[ApiController]
[Authorize]
[Route("api/catalogos")]
[Produces("application/json")]
public class CatalogosController(ObtenerTiposCultivoHandler obtenerTiposCultivo,
    ObtenerTiposActividadHandler obtenerTiposActividad) : ControllerBase
{
    [HttpGet("tipos-cultivo")]
    [ProducesResponseType<IReadOnlyList<TipoCultivoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<TipoCultivoDto>>> ObtenerTiposCultivo(CancellationToken ct) =>
        Ok(await obtenerTiposCultivo.EjecutarAsync(ct));

    [HttpGet("tipos-actividad")]
    [ProducesResponseType<IReadOnlyList<TipoActividadDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<TipoActividadDto>>> ObtenerTiposActividad(CancellationToken ct) =>
    Ok(await obtenerTiposActividad.EjecutarAsync(ct));
}