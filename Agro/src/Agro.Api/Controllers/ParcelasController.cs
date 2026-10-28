using Agro.Application.Parcelas.Commands.EditarParcela;
using Agro.Application.Parcelas.Commands.RegistrarParcela;
using Agro.Application.Parcelas.Common;
using Agro.Application.Parcelas.Queries.ObtenerMisParcelas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
namespace Agro.Api.Controllers;

/// <summary>Parcelas del productor (HU-13). El usuario siempre sale del token, nunca del cuerpo.</summary>
[ApiController]
[Authorize]
[Route("api/parcelas")]
[Produces("application/json")]
public class ParcelasController(
    RegistrarParcelaHandler registrarParcela,
    EditarParcelaHandler editarParcela,
    ObtenerMisParcelasHandler obtenerMisParcelas) : ControllerBase
{
    private int IdUsuario => int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ParcelaDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<ParcelaDto>>> ObtenerMisParcelas(CancellationToken ct) =>
        Ok(await obtenerMisParcelas.EjecutarAsync(IdUsuario, ct));

    [HttpPost]
    [ProducesResponseType<ParcelaDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ParcelaDto>(StatusCodes.Status200OK)]          
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ParcelaDto>> Registrar(RegistrarParcelaCommand comando, CancellationToken ct)
    {
        var (parcela, creada) = await registrarParcela.EjecutarAsync(IdUsuario, comando, ct);
        return creada ? StatusCode(StatusCodes.Status201Created, parcela) : Ok(parcela);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<ParcelaDto>(StatusCodes.Status200OK)]          
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ParcelaDto>> Editar(int id, EditarParcelaCommand comando, CancellationToken ct) =>
        Ok(await editarParcela.EjecutarAsync(id, IdUsuario, comando, ct));

}

