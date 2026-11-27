using Agro.Application.Common.Interfaces;
using Agro.Application.Common.Models;

namespace Agro.Tests.Application.Auth;

/// <summary>Un solo usuario registrado; los códigos se guardan por su hash y son de un solo uso.</summary>
internal sealed class RecuperacionProcedimientosFalsos : IRecuperacionProcedimientos
{
    public const string Correo = "ana@agro.test";

    private readonly Dictionary<string, bool> _codigos = [];   // hash → ¿ya se usó?

    public string? UltimoCorreo { get; private set; }
    public string? HashNuevo { get; private set; }
    public int Solicitudes { get; private set; }

    public Task<CodigoRecuperacion> CrearCodigoAsync(string correo, string codigoHash, CancellationToken ct = default)
    {
        UltimoCorreo = correo;
        if (correo != Correo)
            return Task.FromResult(new CodigoRecuperacion(false, null));          // no está registrado

        if (++Solicitudes > 3)
            return Task.FromResult(new CodigoRecuperacion(false, "Ana"));         // máximo 3 por hora

        _codigos[codigoHash] = false;
        return Task.FromResult(new CodigoRecuperacion(true, "Ana"));
    }

    public Task<bool> RestablecerAsync(string codigoHash, string nuevoHash, CancellationToken ct = default)
    {
        if (!_codigos.TryGetValue(codigoHash, out var usado) || usado)
            return Task.FromResult(false);

        _codigos[codigoHash] = true;
        HashNuevo = nuevoHash;
        return Task.FromResult(true);
    }
}

/// <summary>Guarda los correos en lugar de mandarlos.</summary>
internal sealed class EnvioCorreoFalso : IEnvioCorreo
{
    public List<(string Correo, string Nombre, string Token)> Enviados { get; } = [];

    public Task EnviarRecuperacionAsync(string correo, string nombre, string token, CancellationToken ct = default)
    {
        Enviados.Add((correo, nombre, token));
        return Task.CompletedTask;
    }
    public List<(string Correo, string Nombre, string Token)> Bienvenidas { get; } = [];

    public Task EnviarBienvenidaAsync(string correo, string nombre, string token, CancellationToken ct = default)
    {
        Bienvenidas.Add((correo, nombre, token));
        return Task.CompletedTask;
    }
}