using Agro.Application.Contacto.Commands.EnviarMensajeContacto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Agro.Api.Controllers;

[ApiController]
[Route("api/contacto")]
[Produces("application/json")]
public class ContactoController(EnviarMensajeContactoHandler enviarMensaje) : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting("login")]      // además: máximo 10 por minuto por IP, contra bots
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Enviar(EnviarMensajeContactoCommand comando, CancellationToken ct)
    {
        await enviarMensaje.EjecutarAsync(comando, ct);
        return StatusCode(StatusCodes.Status201Created);
    }
}