using Agro.Application.Common.Interfaces;
using Agro.Application.Common.Models;

namespace Agro.Tests.Application.Auth;

/// <summary>Códigos de verificación en memoria: se guardan por su hash y son de un solo uso.</summary>
internal sealed class VerificacionCorreoProcedimientosFalsos : IVerificacionCorreoProcedimientos
{
    private readonly Dictionary<string, bool> _codigos = [];   // hash → ¿ya se usó?

    public bool YaVerificado { get; set; }
    public bool Verificado { get; private set; }

    public Task<CodigoVerificacionCorreo> CrearCodigoAsync(int idUsuario, string codigoHash, CancellationToken ct = default)
    {
        if (YaVerificado)
            return Task.FromResult(new CodigoVerificacionCorreo(false, "Victor", "victor@correo.com"));

        _codigos[codigoHash] = false;
        return Task.FromResult(new CodigoVerificacionCorreo(true, "Victor", "victor@correo.com"));
    }

    public Task<bool> VerificarAsync(string codigoHash, CancellationToken ct = default)
    {
        if (!_codigos.TryGetValue(codigoHash, out var usado) || usado)
            return Task.FromResult(false);

        _codigos[codigoHash] = true;
        Verificado = true;
        return Task.FromResult(true);
    }
}