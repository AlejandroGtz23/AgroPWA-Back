
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
}
