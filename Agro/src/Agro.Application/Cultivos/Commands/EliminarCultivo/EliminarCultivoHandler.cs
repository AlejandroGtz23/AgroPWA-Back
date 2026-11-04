using Agro.Application.Common.Exceptions;
using Agro.Application.Cultivos.Queries.ObtenerDetalleCultivo;
using Agro.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Cultivos.Commands.EliminarCultivo;

public sealed class EliminarCultivoHandler(ICultivosProcedimientos procedimientos)
{
    public async Task EjecutarAsync(int idCultivo, int idUsuario, CancellationToken ct = default)
    {
        if (!await procedimientos.EliminarAsync(idCultivo, idUsuario, ct))
            throw new NoEncontradoException(ObtenerDetalleCultivoHandler.NoEncontrado);   // 404
    }
}
