using Agro.Application.Common.Exceptions;
using Agro.Application.Cultivos.Queries.ObtenerDetalleCultivo;
using Agro.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Cultivos.Commands.EliminarCultivo;

public sealed class EliminarCultivoHandler(ICultivosProcedimientos procedimientos, IAlmacenFotos almacen)
{
    public async Task EjecutarAsync(int idCultivo, int idUsuario, CancellationToken ct = default)
    {
        // Las rutas se leen antes: al borrar el cultivo, la BD borra sus fotos en cascada
        var fotografias = await procedimientos.ObtenerFotografiasAsync(idCultivo, idUsuario, ct);

        if (!await procedimientos.EliminarAsync(idCultivo, idUsuario, ct))
            throw new NoEncontradoException(ObtenerDetalleCultivoHandler.NoEncontrado);   // 404

        foreach (var foto in fotografias)
            await almacen.EliminarAsync(foto.Url, ct);   // Url trae la ruta guardada en archivo_url
    }
}
