using Agro.Application.Common.Exceptions;
using Agro.Application.Common.Interfaces;
using Agro.Application.Sincronizacion.Common;
using FluentValidation;

namespace Agro.Application.Sincronizacion.Commands.ActualizarPreferencias;

public sealed class ActualizarPreferenciasHandler(
    ISincronizacionProcedimientos procedimientos,
    IValidator<ActualizarPreferenciasCommand> validador)
{
    public const string NoEncontrado = "Dispositivo no encontrado.";

    public async Task<EstadoSincronizacionDto> EjecutarAsync(int idDispositivo, int idUsuario,
        ActualizarPreferenciasCommand comando, CancellationToken ct = default)
    {
        await validador.ValidateAndThrowAsync(comando, ct);                                      // 400

        if (!await procedimientos.ActualizarPreferenciasAsync(idDispositivo, idUsuario,
                comando.SincronizarAuto, comando.SoloWifi, comando.EspacioUsadoMb, ct))
            throw new NoEncontradoException(NoEncontrado);                                        // 404

        return (await procedimientos.ObtenerEstadoAsync(idDispositivo, idUsuario, ct))!;
    }
}
