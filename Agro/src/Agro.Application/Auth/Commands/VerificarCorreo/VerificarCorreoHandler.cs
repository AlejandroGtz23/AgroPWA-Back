using Agro.Application.Auth.Commands.RestablecerContrasena;
using Agro.Application.Common.Interfaces;
using FluentValidation;
using FluentValidation.Results;

namespace Agro.Application.Auth.Commands.VerificarCorreo;

public sealed class VerificarCorreoHandler(
    IVerificacionCorreoProcedimientos procedimientos,
    IGeneradorJwt generadorJwt,
    IValidator<VerificarCorreoCommand> validador)
{
    public async Task EjecutarAsync(VerificarCorreoCommand comando, CancellationToken ct = default)
    {
        await validador.ValidateAndThrowAsync(comando, ct);                                      // 400

        // Se busca por el hash del token, igual que se guardó
        if (!await procedimientos.VerificarAsync(generadorJwt.HashRefreshToken(comando.Token), ct))
            throw new ValidationException([new ValidationFailure("token", RestablecerContrasenaHandler.EnlaceInvalido)]);
    }
}