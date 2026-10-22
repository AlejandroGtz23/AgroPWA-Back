using Agro.Application.Common.Interfaces;
using Agro.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Auth.Common;
/// <summary>/// Crea la sesion/// </summary>
public sealed class CreadorSesion(IGeneradorJwt generadorJwt, IAuthProcedimientos procedimientos, TimeProvider reloj)
{
    private static readonly TimeSpan DuracionRecordarme = TimeSpan.FromDays(7);
    private static readonly TimeSpan DuracionNormal = TimeSpan.FromHours(12);

    public async Task<SesionDto> CrearAsync(UsuarioSesionDto usuario, bool recordarme,
        string? ip, string? agenteUsuario, CancellationToken ct = default)
    {
        var jti = Guid.NewGuid().ToString("N");   // identifica la sesión; 
        var acceso = generadorJwt.GenerarAccessToken(usuario.Id, usuario.Correo, usuario.Rol, jti);
        var (refreshToken, hashRefresh) = generadorJwt.GenerarRefreshToken();
        var vence = reloj.GetUtcNow().UtcDateTime.Add(recordarme ? DuracionRecordarme : DuracionNormal);

        await procedimientos.CrearSesionAsync(
            new NuevaSesion(usuario.Id, jti, hashRefresh, vence, ip, agenteUsuario), ct);

        return new SesionDto(acceso.Token, refreshToken, acceso.ExpiraUtc, usuario);
    }
}