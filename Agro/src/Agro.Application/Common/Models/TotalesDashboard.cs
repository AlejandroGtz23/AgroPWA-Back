using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Common.Models;

public sealed record TotalesDashboard(string Nombre, int CultivosActivos, int AvancesDelMes,
    bool TieneParcela, bool TieneCultivo, bool TieneAvance);
