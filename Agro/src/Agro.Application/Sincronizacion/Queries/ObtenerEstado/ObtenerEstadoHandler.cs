using Agro.Application.Common.Exceptions;
using Agro.Application.Common.Interfaces;
using Agro.Application.Sincronizacion.Commands.ActualizarPreferencias;
using Agro.Application.Sincronizacion.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Sincronizacion.Queries.ObtenerEstado;

public sealed class ObtenerEstadoHandler(ISincronizacionProcedimientos procedimientos)
{
    public async Task<EstadoSincronizacionDto> EjecutarAsync(int idDispositivo, int idUsuario, CancellationToken ct = default) =>
        await procedimientos.ObtenerEstadoAsync(idDispositivo, idUsuario, ct)
        ?? throw new NoEncontradoException(ActualizarPreferenciasHandler.NoEncontrado);           // 404
}