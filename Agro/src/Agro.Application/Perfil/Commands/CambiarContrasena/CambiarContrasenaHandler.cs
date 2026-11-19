using Agro.Application.Common.Exceptions;
using Agro.Application.Common.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using Agro.Application.Perfil.Queries.ObtenerPerfil;
using System.Text;

namespace Agro.Application.Perfil.Commands.CambiarContrasena;

public sealed class CambiarContrasenaHandler(
    IPerfilProcedimientos procedimientos,
    IHashContrasena hashContrasena,
    IValidator<CambiarContrasenaCommand> validador)
{
    public const string ActualIncorrecta = "La contraseña actual no es correcta.";

    /// <param name="jtiActual">Sesión desde la que se cambia: sigue abierta; las demás se cierran.</param>
    public async Task EjecutarAsync(int idUsuario, string jtiActual, CambiarContrasenaCommand comando,
        CancellationToken ct = default)
    {
        await validador.ValidateAndThrowAsync(comando, ct);                                 // 400

        var hashActual = await procedimientos.ObtenerHashAsync(idUsuario, ct)
                         ?? throw new NoEncontradoException(ObtenerPerfilHandler.NoEncontrado);   // 404

        if (!hashContrasena.Verificar(comando.ContrasenaActual, hashActual))
            throw new ValidationException([new ValidationFailure("contrasenaActual", ActualIncorrecta)]);   // 400

        await procedimientos.CambiarContrasenaAsync(idUsuario, hashContrasena.Generar(comando.ContrasenaNueva),
            jtiActual, ct);
    }
}