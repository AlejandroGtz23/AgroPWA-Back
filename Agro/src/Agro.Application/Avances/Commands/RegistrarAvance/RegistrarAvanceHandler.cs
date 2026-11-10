using Agro.Application.Avances.Common;
using Agro.Application.Common.Interfaces;
using Agro.Application.Common.Models;
using Agro.Application.Cultivos.Common;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Avances.Commands.RegistrarAvance;

public sealed class RegistrarAvanceHandler(
    IAvancesProcedimientos procedimientos,
    IValidator<RegistrarAvanceCommand> validador)
{
    public async Task<(AvanceDto Avance, bool Creado)> EjecutarAsync(int idCultivo, int idUsuario,
        RegistrarAvanceCommand comando, CancellationToken ct = default)
    {
        await validador.ValidateAndThrowAsync(comando, ct);                                // 400

        var datos = new NuevoAvance(
            comando.IdTipoActividad!.Value, comando.FechaActividad!.Value, comando.Descripcion.Trim(),
            comando.DuracionMin, ReglasCultivo.Limpiar(comando.MetodoRiego),
            ReglasCultivo.Limpiar(comando.Producto), comando.Cantidad, ReglasCultivo.Limpiar(comando.Unidad));

        // 404, 409 o 400 por campo según lo que indique el procedimiento
        var registro = await procedimientos.RegistrarAsync(idUsuario, idCultivo, comando.IdLocal, datos, ct);
        var avance = await procedimientos.ObtenerAsync(registro.IdAvance, idUsuario, ct);
        return (avance!, !registro.YaExistia);
    }
}
