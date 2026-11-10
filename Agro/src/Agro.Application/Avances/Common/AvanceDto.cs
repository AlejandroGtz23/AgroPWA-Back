using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Avances.Common;

public sealed record AvanceDto(
    int Id, Guid? IdLocal, int IdCultivo, int IdTipoActividad, string Tipo,
    DateTimeOffset FechaActividad, string Descripcion,
    int? DuracionMin, string? MetodoRiego, string? Producto, decimal? Cantidad, string? Unidad,
    string EstadoSincronizacion);