using Agro.Application.Common.Interfaces;
using Agro.Application.Common.Models;
using Agro.Application.Perfil.Common;

namespace Agro.Tests.Application.Perfil;

/// <summary>Un solo usuario en memoria con su perfil, su hash y la sesión que se conserva al cambiar la contraseña.</summary>
internal sealed class PerfilProcedimientosFalsos : IPerfilProcedimientos
{
    public const int Usuario = 1;

    public PerfilDto Perfil { get; private set; } =
        new("Ana", "López", null, "ana@agro.test", null, null, "productor", DateTimeOffset.UnixEpoch, false);
    public string Hash { get; private set; } = "hash:Actual123";
    public string? JtiConservado { get; private set; }

    public Task<PerfilDto?> ObtenerAsync(int idUsuario, CancellationToken ct = default) =>
        Task.FromResult<PerfilDto?>(idUsuario == Usuario ? Perfil : null);

    public Task<bool> ActualizarAsync(int idUsuario, DatosPerfil datos, CancellationToken ct = default)
    {
        if (idUsuario != Usuario)
            return Task.FromResult(false);

        Perfil = Perfil with
        {
            Nombre = datos.Nombre,
            ApellidoPaterno = datos.ApellidoPaterno,
            ApellidoMaterno = datos.ApellidoMaterno,
            Telefono = datos.Telefono,
            ComunidadMunicipio = datos.ComunidadMunicipio
        };
        return Task.FromResult(true);
    }

    public Task<string?> ObtenerHashAsync(int idUsuario, CancellationToken ct = default) =>
        Task.FromResult<string?>(idUsuario == Usuario ? Hash : null);

    public Task<bool> CambiarContrasenaAsync(int idUsuario, string nuevoHash, string jtiActual, CancellationToken ct = default)
    {
        if (idUsuario != Usuario)
            return Task.FromResult(false);

        Hash = nuevoHash;
        JtiConservado = jtiActual;
        return Task.FromResult(true);
    }
}