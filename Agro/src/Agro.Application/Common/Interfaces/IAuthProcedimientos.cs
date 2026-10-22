
using Agro.Application.Auth.Common;
using Agro.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Common.Interfaces;
/// <summary>
/// Operaciones que se ejecutan con prodedimientos almacenados
/// </summary>
public interface IAuthProcedimientos
{
    Task<int> RegistrarUsuarioAsync(NuevoUsuario datos, CancellationToken ct = default);

    /// <summary>sp_obtener_usuario_login: datos del usuario activo con ese correo, o null si no existe.</summary>
    Task<UsuarioLogin?> ObtenerUsuarioLoginAsync(string correo, CancellationToken ct = default);

    /// <summary>sp_registrar_intento_acceso: guarda el intento y aplica el bloqueo tras 5 fallos.</summary>
    Task RegistrarIntentoAccesoAsync(string correo, int? idUsuario, bool exitoso,
        string? ip, string? agenteUsuario, CancellationToken ct = default);

    /// <summary>sp_crear_sesion: crea la sesión y devuelve su id.</summary>
    Task<int> CrearSesionAsync(NuevaSesion sesion, CancellationToken ct = default);

    /// <summary>sp_validar_sesion: true si la sesión del jti sigue abierta (y actualiza su actividad).</summary>
    Task<bool> ValidarSesionAsync(string jti, int minutosInactividad, CancellationToken ct = default);

    /// <summary>sp_renovar_sesion: rota jti y refresh token. Devuelve el usuario, o null si la sesión ya no es válida.</summary>
    Task<UsuarioSesionDto?> RenovarSesionAsync(string refreshTokenHash, string nuevoJti,
        string nuevoRefreshTokenHash, int minutosInactividad, CancellationToken ct = default);
}
