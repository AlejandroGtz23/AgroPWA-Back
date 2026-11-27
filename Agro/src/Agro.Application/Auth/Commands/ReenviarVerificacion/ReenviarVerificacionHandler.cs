using Agro.Application.Auth.Common;

namespace Agro.Application.Auth.Commands.ReenviarVerificacion;

/// <summary>Manda otro enlace al usuario del token. Si ya verificó o pidió demasiados, no manda nada (siempre 204).</summary>
public sealed class ReenviarVerificacionHandler(EnviadorVerificacionCorreo enviadorVerificacion)
{
    public Task EjecutarAsync(int idUsuario, CancellationToken ct = default) =>
        enviadorVerificacion.EnviarAsync(idUsuario, ct);
}