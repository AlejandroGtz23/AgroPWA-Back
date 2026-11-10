using Agro.Application.Avances.Common;
using Agro.Application.Common.Interfaces;
using Agro.Application.Cultivos.Common;
using Agro.Application.Catalogos.Queries.ObtenerTiposActividad;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Catalogos.Queries.ObtenerTiposActividad;

public sealed class ObtenerTiposActividadHandler(IAvancesProcedimientos procedimientos)
{
    public Task<IReadOnlyList<TipoActividadDto>> EjecutarAsync(CancellationToken ct = default) => procedimientos.ObtenerTiposAsync(ct);
}
