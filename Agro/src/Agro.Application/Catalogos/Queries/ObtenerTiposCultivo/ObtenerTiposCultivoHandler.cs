using Agro.Application.Common.Interfaces;
using Agro.Application.Cultivos.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Catalogos.Queries.ObtenerTiposCultivo;

public sealed class ObtenerTiposCultivoHandler(ICultivosProcedimientos procedimientos)
{
    public Task<IReadOnlyList<TipoCultivoDto>> EjecutarAsync(CancellationToken ct = default) => procedimientos.ObtenerTiposAsync(ct);
}
