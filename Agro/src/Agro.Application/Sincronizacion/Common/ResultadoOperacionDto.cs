using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Sincronizacion.Common;

public sealed record ResultadoOperacionDto(Guid IdLocal, string Entidad, string Operacion, string Estado, int? Id, string? MensajeError);

public sealed record ResultadoSincronizacionDto(int Sincronizados, int Errores,
    IReadOnlyList<ResultadoOperacionDto> Resultados);