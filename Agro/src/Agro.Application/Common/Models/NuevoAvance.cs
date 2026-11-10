using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Common.Models;

public sealed record NuevoAvance(int IdTipoActividad, DateTimeOffset FechaActividad, string Descripcion,
    int? DuracionMin, string? MetodoRiego, string? Producto, decimal? Cantidad, string? Unidad);
