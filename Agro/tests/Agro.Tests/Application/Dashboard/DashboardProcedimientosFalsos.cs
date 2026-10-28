using Agro.Application.Common.Interfaces;
using Agro.Application.Common.Models;
using Agro.Application.Dashboard.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Tests.Application.Dashboard;

internal  sealed class DashboardProcedimientosFalsos : IDashboardProcedimientos
{
    public TotalesDashboard? Totales { get; set; }
    public List<ActividadRecienteDto> Actividad { get; } = [];
    public int? UsuarioConsultado { get; private set; }
    public int? LimitePedido { get; private set; }

    public Task<TotalesDashboard?> ObtenerTotalesAsync(int idUsuario, CancellationToken ct = default)
    {
        UsuarioConsultado = idUsuario;
        return Task.FromResult(Totales);
    }
    public Task<IReadOnlyList<ActividadRecienteDto>> ObtenerActividadRecienteAsync(int idUsuario, int limite,
        CancellationToken ct = default)
    {
        LimitePedido = limite;
        return Task.FromResult<IReadOnlyList<ActividadRecienteDto>>(Actividad);
    }
}
