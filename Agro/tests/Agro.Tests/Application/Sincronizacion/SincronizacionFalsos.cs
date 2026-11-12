using Agro.Application.Common.Interfaces;
using Agro.Application.Common.Models;
using Agro.Application.Sincronizacion.Common;

namespace Agro.Tests.Application.Sincronizacion;

/// <summary>Dispositivo y cola en memoria. IdsEnBase simula registros sincronizados en envíos anteriores.</summary>
internal sealed class SincronizacionProcedimientosFalsos : ISincronizacionProcedimientos
{
    public const int MiDispositivo = 5;

    public List<(string Entidad, Guid IdLocal, string Operacion, string Estado, string? Mensaje)> Cola { get; } = [];
    public Dictionary<(string Entidad, Guid IdLocal), int> IdsEnBase { get; } = [];
    public bool Marcado { get; private set; }

    public Task<DispositivoRegistrado> RegistrarDispositivoAsync(int idUsuario, string identificador, string nombre,
        CancellationToken ct = default) =>
        Task.FromResult(new DispositivoRegistrado(MiDispositivo, YaExistia: false));

    public Task<bool> ActualizarPreferenciasAsync(int idDispositivo, int idUsuario, bool sincronizarAuto, bool soloWifi,
        decimal? espacioUsadoMb, CancellationToken ct = default) =>
        Task.FromResult(idDispositivo == MiDispositivo);

    public Task<EstadoSincronizacionDto?> ObtenerEstadoAsync(int idDispositivo, int idUsuario, CancellationToken ct = default) =>
        Task.FromResult(idDispositivo == MiDispositivo
            ? new EstadoSincronizacionDto(idDispositivo, "Prueba", true, false, 0, null, Cola.Count(c => c.Estado == "error"))
            : null);

    public Task RegistrarResultadoAsync(int idUsuario, int idDispositivo, string entidad, Guid idLocal, string operacion,
        string datosJson, string estado, string? mensajeError, CancellationToken ct = default)
    {
        Cola.Add((entidad, idLocal, operacion, estado, mensajeError));
        return Task.CompletedTask;
    }

    public Task MarcarSincronizacionAsync(int idDispositivo, int idUsuario, CancellationToken ct = default)
    {
        Marcado = true;
        return Task.CompletedTask;
    }

    public Task<int?> ObtenerIdPorLocalAsync(int idUsuario, string entidad, Guid idLocal, CancellationToken ct = default) =>
        Task.FromResult(IdsEnBase.TryGetValue((entidad, idLocal), out var id) ? id : (int?)null);
}