using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Agro.Application.Sincronizacion.Commands.ProcesarSincronizacion;

public sealed record ProcesarSincronizacionCommand(int? IdDispositivo, IReadOnlyList<OperacionSincronizacion> Operaciones);

public sealed record OperacionSincronizacion(string Entidad, Guid? IdLocal, string Operacion, JsonElement Datos,
    DateTimeOffset? FechaCreacion);