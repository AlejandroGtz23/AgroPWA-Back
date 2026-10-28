using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Dashboard.Common;

public sealed record ResumenDashboardDto(string Nombre, int CultivosActivos, int AvancesDelMes,
    GuiaDashboardDto Guia, IReadOnlyList<ActividadRecienteDto> ActividadReciente);

public sealed record GuiaDashboardDto(bool TieneParcela, bool TieneCultivo, bool TieneAvance);

public sealed record ActividadRecienteDto(string Tipo, string Cultivo, DateTime Fecha);
