using Agro.Application.Common.Exceptions;
using Agro.Application.Common.Interfaces;
using Agro.Application.Common.Models;
using Agro.Application.Cultivos.Common;
using Agro.Application.Cultivos.Queries.ObtenerDetalleCultivo;
using FluentValidation;
using Agro.Application.Fotografias.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Cultivos.Commands.EditarCultivo;

public sealed class EditarCultivoHandler(ICultivosProcedimientos procedimientos,
    IValidator<EditarCultivoCommand> validador, EnlacesFotografias enlaces)
{
    public async Task<CultivoDto> EjecutarAsync(int idCultivo, int idUsuario,
        EditarCultivoCommand comando, CancellationToken ct = default)
    {
        await validador.ValidateAndThrowAsync(comando, ct);                              // 400

        var cambios = new CultivoEditado(
            new NuevoCultivo(
                comando.IdParcela!.Value, comando.IdTipoCultivo!.Value, comando.Nombre.Trim(),
                ReglasCultivo.Limpiar(comando.Variedad), comando.FechaSiembra!.Value,
                ReglasCultivo.Limpiar(comando.Observaciones)),
            comando.Estado!.Trim().ToLowerInvariant(),
            comando.FechaCosecha,
            comando.FechaModificacion);

        // 400 si la parcela no es del usuario o el tipo no existe (lo indica el procedimiento)
        if (!await procedimientos.EditarAsync(idCultivo, idUsuario, cambios, ct))
            throw new NoEncontradoException(ObtenerDetalleCultivoHandler.NoEncontrado);   // 404

        // Si el cambio era más viejo que el guardado, se devuelve la versión del servidor
        return enlaces.ConEnlaces((await procedimientos.ObtenerDetalleAsync(idCultivo, idUsuario, ct))!.Cultivo);
    }
}
