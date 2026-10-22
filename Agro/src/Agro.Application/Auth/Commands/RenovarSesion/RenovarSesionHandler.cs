using Agro.Application.Auth.Common;
using Agro.Application.Common.Exceptions;
using Agro.Application.Common.Interfaces;
using FluentValidation;

namespace Agro.Application.Auth.Commands.RenovarSesion;

/// <summary>Command: entrega tokens nuevos con el refresh token, sin pedir la contraseña.</summary>
public sealed class RenovarSesionHandler(
    IAuthProcedimientos procedimientos,
    IGeneradorJwt generadorJwt,
    IValidator<RenovarSesionCommand> validador)
{
    public async Task<SesionDto> EjecutarAsync(RenovarSesionCommand comando, CancellationToken ct = default)
    {
        await validador.ValidateAndThrowAsync(comando, ct);

        // En la base solo existe el hash del refresh token: se busca por su hash
        var hashActual = generadorJwt.HashRefreshToken(comando.RefreshToken);
        var nuevoJti = Guid.NewGuid().ToString("N");
        var (nuevoRefreshToken, nuevoHash) = generadorJwt.GenerarRefreshToken();

        // sp_renovar_sesion: valida, rota jti y refresh token, y devuelve el usuario
        var usuario = await procedimientos.RenovarSesionAsync(
                          hashActual, nuevoJti, nuevoHash, ReglasSesion.MinutosInactividad, ct)
                      ?? throw new NoAutorizadoException("Tu sesión expiró. Inicia sesión de nuevo.");

        var acceso = generadorJwt.GenerarAccessToken(usuario.Id, usuario.Correo, usuario.Rol, nuevoJti);
        return new SesionDto(acceso.Token, nuevoRefreshToken, acceso.ExpiraUtc, usuario);
    }
}