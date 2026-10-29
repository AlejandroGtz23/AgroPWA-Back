using Agro.Application.Common.Exceptions;
using Agro.Application.Common.Interfaces;
using Agro.Application.Cultivos.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Cultivos.Queries.ObtenerDetalleCultivo;

public sealed class ObtenerDetalleCultivoHandler(ICultivosProcedimientos procedimientos)
{
    public const string NoEncontrado = "Cultivo no encontrado.";
    public async Task<DetalleCultivoDto> EjecutarAsync(int idCultivo, int idUsuario, CancellationToken ct = default)
    {
        var datos = await procedimientos.ObtenerDetalleAsync(idCultivo, idUsuario, ct) ?? throw new NoEncontradoException(NoEncontrado);                         // 404

        var avances = await procedimientos.ObtenerAvancesAsync(idCultivo, idUsuario, ct);      // más reciente primero
        var fotografias = await procedimientos.ObtenerFotografiasAsync(idCultivo, idUsuario, ct);

        return new DetalleCultivoDto(datos.Cultivo, avances, fotografias, new TotalesCultivoDto(datos.TotalAvances, datos.TotalFotografias));
    }
}
