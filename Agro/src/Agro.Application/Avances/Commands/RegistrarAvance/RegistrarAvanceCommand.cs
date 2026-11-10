using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Avances.Commands.RegistrarAvance;

public sealed record RegistrarAvanceCommand(Guid? IdLocal, int? IdTipoActividad, DateTimeOffset? FechaActividad,
    string Descripcion, int? DuracionMin, string? MetodoRiego, string? Producto, decimal? Cantidad, string? Unidad);