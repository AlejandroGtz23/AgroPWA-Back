using Agro.Application.Common.Interfaces;
using FluentValidation;
using FluentValidation.Results;

namespace Agro.Application.Auth.Commands.RestablecerContrasena;

public sealed class RestablecerContrasenaHandler(
    IRecuperacionProcedimientos procedimientos,
    IGeneradorJwt generadorJwt,
    IHashContrasena hashContrasena,
    IValidator<RestablecerContrasenaCommand> validador)
{
    public const string EnlaceInvalido = "El enlace expiró o ya se usó. Solicita uno nuevo.";

    public async Task EjecutarAsync(RestablecerContrasenaCommand comando, CancellationToken ct = default)
    {
        await validador.ValidateAndThrowAsync(comando, ct);                                     // 400

        // Se busca por el hash del token, igual que se guardó
        var restablecido = await procedimientos.RestablecerAsync(
            generadorJwt.HashRefreshToken(comando.Token), hashContrasena.Generar(comando.ContrasenaNueva), ct);

        if (!restablecido)
            throw new ValidationException([new ValidationFailure("token", EnlaceInvalido)]);   // 400
    }
}