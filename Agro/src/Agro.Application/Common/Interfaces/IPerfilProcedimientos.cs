using Agro.Application.Common.Models;
using Agro.Application.Perfil.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Common.Interfaces;

public interface IPerfilProcedimientos
{
    /// <summary>null si el usuario no existe o está dado de baja.</summary>
    Task<PerfilDto?> ObtenerAsync(int idUsuario, CancellationToken ct = default);

    /// <summary>false si el usuario no existe (404).</summary>
    Task<bool> ActualizarAsync(int idUsuario, DatosPerfil datos, CancellationToken ct = default);

    /// <summary>Hash BCrypt actual, para verificar la contraseña actual; null si no existe.</summary>
    Task<string?> ObtenerHashAsync(int idUsuario, CancellationToken ct = default);

    /// <summary>Guarda el hash nuevo y cierra las demás sesiones (la del jti actual sigue abierta).</summary>
    Task<bool> CambiarContrasenaAsync(int idUsuario, string nuevoHash, string jtiActual, CancellationToken ct = default);
}