using Agro.Application.Auth.Common;
using Agro.Application.Common.Exceptions;
using Agro.Application.Common.Interfaces;
using Agro.Application.Common.Models;

namespace Agro.Tests.Application.Auth;

/// <summary>
/// Simula los procedimientos almacenados sin base de datos real.
/// Guarda lo que recibe para que las pruebas puedan revisarlo.
/// </summary>
internal sealed class ProcedimientosFalsos : IAuthProcedimientos
{
    public UsuarioLogin? UsuarioLogin { get; set; }
    public UsuarioSesionDto? UsuarioRenovado { get; set; }
    public bool CorreoDuplicado { get; set; }
    public bool SesionValida { get; set; } = true;
    public bool SesionCerrada { get; set; } = true;

    public NuevoUsuario? UsuarioRegistrado { get; private set; }
    public NuevaSesion? SesionCreada { get; private set; }
    public string? HashRenovado { get; private set; }
    public int? MinutosRecibidos { get; private set; }
    public List<bool> Intentos { get; } = [];

    public Task<int> RegistrarUsuarioAsync(NuevoUsuario datos, CancellationToken ct = default)
    {
        if (CorreoDuplicado)
            throw new ConflictoException("Ya existe una cuenta con ese correo.");

        UsuarioRegistrado = datos;
        return Task.FromResult(7);
    }

    public Task<UsuarioLogin?> ObtenerUsuarioLoginAsync(string correo, CancellationToken ct = default)
        => Task.FromResult(UsuarioLogin);

    public Task RegistrarIntentoAccesoAsync(string correo, int? idUsuario, bool exitoso,
        string? ip, string? agenteUsuario, CancellationToken ct = default)
    {
        Intentos.Add(exitoso);
        return Task.CompletedTask;
    }

    public Task<int> CrearSesionAsync(NuevaSesion sesion, CancellationToken ct = default)
    {
        SesionCreada = sesion;
        return Task.FromResult(1);
    }

    public Task<bool> ValidarSesionAsync(string jti, int minutosInactividad, CancellationToken ct = default)
    {
        MinutosRecibidos = minutosInactividad;
        return Task.FromResult(SesionValida);
    }

    public Task<UsuarioSesionDto?> RenovarSesionAsync(string refreshTokenHash, string nuevoJti,
        string nuevoRefreshTokenHash, int minutosInactividad, CancellationToken ct = default)
    {
        HashRenovado = refreshTokenHash;
        return Task.FromResult(UsuarioRenovado);
    }

    public Task<bool> CerrarSesionAsync(string jti, CancellationToken ct = default)
        => Task.FromResult(SesionCerrada);
}

/// <summary>Tokens fijos para poder comparar el resultado.</summary>
internal sealed class GeneradorJwtFalso : IGeneradorJwt
{
    public TokenGenerado GenerarAccessToken(int idUsuario, string correo, string rol, string jti)
        => new("token-acceso", DateTime.UtcNow.AddMinutes(20));

    public (string Token, string Hash) GenerarRefreshToken() => ("refresh-nuevo", "hash-refresh-nuevo");

    public string HashRefreshToken(string refreshToken) => "hash-" + refreshToken;
}

/// <summary>"Hash" simple para que las pruebas sean rápidas (BCrypt real se prueba aparte).</summary>
internal sealed class HashFalso : IHashContrasena
{
    public string Generar(string contrasena) => "hash:" + contrasena;

    public bool Verificar(string contrasena, string hash) => hash == "hash:" + contrasena;
}
