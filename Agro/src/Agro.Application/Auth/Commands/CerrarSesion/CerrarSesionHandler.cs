using Agro.Application.Common.Exceptions;
using Agro.Application.Common.Interfaces;

namespace Agro.Application.Auth.Commands.CerrarSesion;

/// <summary>Command: cierra la sesión en el servidor (HU-11, criterio 1).
/// Desde ese momento el access token y el refresh token dejan de servir.</summary>
public sealed class CerrarSesionHandler(IAuthProcedimientos procedimientos)
{
    public async Task EjecutarAsync(CerrarSesionCommand comando, CancellationToken ct = default)
    {
        if (!await procedimientos.CerrarSesionAsync(comando.Jti, ct))   // sp_cerrar_sesion
            throw new NoAutorizadoException("La sesión ya estaba cerrada.");
    }
}