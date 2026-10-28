using Agro.Application.Common.Models;
using Agro.Application.Dashboard.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Common.Interfaces;

public interface IDashboardProcedimientos
{
    Task<TotalesDashboard?> ObtenerTotalesAsync(int idUsuario, CancellationToken ct = default);

    Task<IReadOnlyList<ActividadRecienteDto>> ObtenerActividadRecienteAsync(int idUsuario, int limite, CancellationToken ct = default);
}
