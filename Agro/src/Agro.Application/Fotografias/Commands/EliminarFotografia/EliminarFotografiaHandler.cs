using Agro.Application.Common.Exceptions;
using Agro.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Fotografias.Commands.EliminarFotografia;

public sealed class EliminarFotografiaHandler(IFotografiasProcedimientos procedimientos, IAlmacenFotos almacen)
{
    public const string NoEncontrada = "Fotografía no encontrada.";

    public async Task EjecutarAsync(int idFotografia, int idUsuario, CancellationToken ct = default)
    {
        // Primero la BD: si borrar el archivo fallara, la foto ya no aparece en la app
        var ruta = await procedimientos.EliminarAsync(idFotografia, idUsuario, ct)
                   ?? throw new NoEncontradoException(NoEncontrada);                     // 404
        await almacen.EliminarAsync(ruta, ct);
    }
}
