using Agro.Application.Auth.Common;
using Agro.Application.Common.Interfaces;
using FluentValidation;

namespace Agro.Application.Auth.Commands.SolicitarRecuperacion;

public sealed class SolicitarRecuperacionHandler(
    IRecuperacionProcedimientos procedimientos,
    IGeneradorJwt generadorJwt,
    IEnvioCorreo envioCorreo,
    IValidator<SolicitarRecuperacionCommand> validador)
{
    /// <summary>Siempre la misma respuesta, exista o no el correo: así no se revela quién está registrado.</summary>
    public const string Respuesta = "Si el correo está registrado, te enviamos un enlace para restablecer tu contraseña.";

    public async Task<RespuestaRecuperacionDto> EjecutarAsync(SolicitarRecuperacionCommand comando, CancellationToken ct = default)
    {
        await validador.ValidateAndThrowAsync(comando, ct);                                     // 400

        var correo = comando.Correo.Trim().ToLowerInvariant();

        // Token aleatorio de un solo uso: el usuario recibe el token y la BD guarda solo su hash
        var (token, hash) = generadorJwt.GenerarRefreshToken();
        var codigo = await procedimientos.CrearCodigoAsync(correo, hash, ct);

        if (codigo.Enviar)
            await envioCorreo.EnviarRecuperacionAsync(correo, codigo.Nombre!, token, ct);

        return new RespuestaRecuperacionDto(Respuesta);
    }
}