using Agro.Application.Common.Models;
using Agro.Application.Sincronizacion.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Common.Interfaces;

public interface ISincronizacionProcedimientos
{
    Task<DispositivoRegistrado> RegistrarDispositivoAsync(int idUsuario, string identificador, string nombre,
        CancellationToken ct = default);

    Task<bool> ActualizarPreferenciasAsync(int idDispositivo, int idUsuario, bool sincronizarAuto, bool soloWifi,
        decimal? espacioUsadoMb, CancellationToken ct = default);

    Task<EstadoSincronizacionDto?> ObtenerEstadoAsync(int idDispositivo, int idUsuario, CancellationToken ct = default);

    Task RegistrarResultadoAsync(int idUsuario, int idDispositivo, string entidad, Guid idLocal, string operacion,
        string datosJson, string estado, string? mensajeError, CancellationToken ct = default);

    Task MarcarSincronizacionAsync(int idDispositivo, int idUsuario, CancellationToken ct = default);

    /// <summary>id del servidor de un registro creado sin conexión; null si no existe.</summary>
    Task<int?> ObtenerIdPorLocalAsync(int idUsuario, string entidad, Guid idLocal, CancellationToken ct = default);
}
