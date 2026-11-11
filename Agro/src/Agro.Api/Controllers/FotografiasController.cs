using Agro.Application.Common.Models;
using Agro.Application.Fotografias.Commands.EliminarFotografia;
using Agro.Application.Fotografias.Commands.SubirFotografia;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Agro.Api.Controllers;

[ApiController]
[Authorize]
[Route("api")]
[Produces("application/json")]
public class FotografiasController(
    SubirFotografiaHandler subirFotografia, 
    EliminarFotografiaHandler eliminarFotografia) : ControllerBase
{
    private int IdUsuario => int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    [HttpPost("cultivos/{idCultivo:int}/fotografias")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6 * 1024 * 1024)]   // corta antes los envíos enormes (el validador revisa los 5 MB)
    [ProducesResponseType<FotografiaSubidaDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<FotografiaSubidaDto>(StatusCodes.Status200OK)]          // ya existía (mismo idLocal)
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FotografiaSubidaDto>> Subir(int idCultivo, [FromForm] SubirFotografiaFormulario formulario,
        CancellationToken ct)
    {
        await using var contenido = formulario.Archivo?.OpenReadStream() ?? Stream.Null;
        var comando = new SubirFotografiaCommand(contenido, formulario.Archivo?.Length ?? 0, formulario.IdAvance,
            formulario.EsPortada, formulario.IdLocal, formulario.Latitud, formulario.Longitud);

        var (fotografia, creada) = await subirFotografia.EjecutarAsync(idCultivo, IdUsuario, comando, ct);
        return creada ? StatusCode(StatusCodes.Status201Created, fotografia) : Ok(fotografia);
    }

    [HttpDelete("fotografias/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await eliminarFotografia.EjecutarAsync(id, IdUsuario, ct);
        return NoContent();
    }
}

/// <summary>Campos del formulario multipart de la foto.</summary>
public sealed class SubirFotografiaFormulario
{
    public IFormFile? Archivo { get; set; }
    public int? IdAvance { get; set; }
    public bool EsPortada { get; set; }
    public Guid? IdLocal { get; set; }
    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }
}