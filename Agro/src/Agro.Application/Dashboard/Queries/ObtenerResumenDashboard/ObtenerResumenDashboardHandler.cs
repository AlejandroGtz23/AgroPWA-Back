using Agro.Application.Common.Exceptions;
using Agro.Application.Common.Interfaces;
using Agro.Application.Dashboard.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Dashboard.Queries.ObtenerResumenDashboard;

public sealed class ObtenerResumenDashboardHandler(IDashboardProcedimientos procedimientos)
{
    public const int LimiteActividad = 5;

    public async Task<ResumenDashboardDto> EjecutarAsync(int idUsuario, CancellationToken ct = default)
    {
        var totales = await procedimientos.ObtenerTotalesAsync(idUsuario, ct)
                      ?? throw new NoEncontradoException("El usuario no existe.");   // 404

        var actividad = await procedimientos.ObtenerActividadRecienteAsync(idUsuario, LimiteActividad, ct);

        return new ResumenDashboardDto(
            totales.Nombre, totales.CultivosActivos, totales.AvancesDelMes,
            new GuiaDashboardDto(totales.TieneParcela, totales.TieneCultivo, totales.TieneAvance),
            actividad);
    }
}
