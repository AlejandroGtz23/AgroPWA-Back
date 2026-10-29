using Agro.Application.Common.Interfaces;
using Agro.Application.Common.Models;
using Agro.Application.Cultivos.Common;
using FluentValidation;

namespace Agro.Application.Cultivos.Queries.ObtenerMisCultivos;

public sealed class ObtenerMisCultivosHandler(
    ICultivosProcedimientos procedimientos,
    IValidator<ObtenerMisCultivosQuery> validador)
{
    public async Task<Pagina<CultivoResumenDto>> EjecutarAsync(int idUsuario, ObtenerMisCultivosQuery consulta,
        CancellationToken ct = default)
    {
        await validador.ValidateAndThrowAsync(consulta, ct);                     // 400

        var filtro = new FiltroCultivos(
            string.IsNullOrWhiteSpace(consulta.Buscar) ? null : consulta.Buscar.Trim(),
            string.IsNullOrWhiteSpace(consulta.Estado) ? null : consulta.Estado.Trim().ToLowerInvariant(),
            consulta.IdTipoCultivo, consulta.Pagina, consulta.Tamano);

        return await procedimientos.ObtenerAsync(idUsuario, filtro, ct);
    }
}