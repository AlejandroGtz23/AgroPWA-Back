using Agro.Application.Common.Interfaces;
using Agro.Application.Sincronizacion.Common;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Sincronizacion.Commands.RegistrarDispositivo;

public sealed class RegistrarDispositivoHandler(
    ISincronizacionProcedimientos procedimientos,
    IValidator<RegistrarDispositivoCommand> validador)
{
    public async Task<(EstadoSincronizacionDto Dispositivo, bool Creado)> EjecutarAsync(int idUsuario,
        RegistrarDispositivoCommand comando, CancellationToken ct = default)
    {
        await validador.ValidateAndThrowAsync(comando, ct);                                      // 400

        var registro = await procedimientos.RegistrarDispositivoAsync(
            idUsuario, comando.Identificador.Trim(), comando.Nombre.Trim(), ct);

        var dispositivo = await procedimientos.ObtenerEstadoAsync(registro.IdDispositivo, idUsuario, ct);
        return (dispositivo!, !registro.YaExistia);
    }
}