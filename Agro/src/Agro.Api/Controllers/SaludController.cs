using Agro.Application.Salud.Queries.ComprobarSalud;
using Microsoft.AspNetCore.Mvc;

namespace Agro.Api.Controllers;

/// <summary>Servicio de prueba para saber si la API y la base de datos están activas (HU-04).</summary>
[ApiController]
[Route("api/salud")]
[Produces("application/json")]
public class SaludController(ComprobarSaludHandler comprobarSalud) : ControllerBase
{
    /// <summary>200 con estado «OK», o 503 si no hay conexión con la base de datos.</summary>
    [HttpGet]
    [ProducesResponseType<SaludDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<SaludDto>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<SaludDto>> Get(CancellationToken cancellationToken)
    {
        var salud = await comprobarSalud.EjecutarAsync(cancellationToken);
        return salud.Estado == "OK"
            ? Ok(salud)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, salud);
    }
}
