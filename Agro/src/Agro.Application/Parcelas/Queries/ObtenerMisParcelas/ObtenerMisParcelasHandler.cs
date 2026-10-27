using Agro.Application.Common.Interfaces;
using Agro.Application.Parcelas.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Parcelas.Queries.ObtenerMisParcelas;

public sealed class ObtenerMisParcelasHandler(IParcelasProcedimientos procedimientos)
{
    public Task<IReadOnlyList<ParcelaDto>> EjecutarAsync(int idUsuario, CancellationToken ct = default) =>
        procedimientos.ObtenerAsync(idUsuario, null, ct);
}
