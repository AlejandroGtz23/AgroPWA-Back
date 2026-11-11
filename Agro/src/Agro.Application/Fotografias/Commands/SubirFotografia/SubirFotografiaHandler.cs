using Agro.Application.Common.Interfaces;
using Agro.Application.Common.Models;
using Agro.Application.Fotografias.Common;
using FluentValidation;
using FluentValidation.Results;
using System;


namespace Agro.Application.Fotografias.Commands.SubirFotografia;

public sealed class SubirFotografiaHandler(
    IFotografiasProcedimientos procedimientos,
    IAlmacenFotos almacen,
    IValidator<SubirFotografiaCommand> validador)
{
    public async Task<(FotografiaSubidaDto Fotografia, bool Creada)> EjecutarAsync(int idCultivo, int idUsuario,
        SubirFotografiaCommand comando, CancellationToken ct = default)
    {
        await validador.ValidateAndThrowAsync(comando, ct);                                    // 400

        var tipo = await TipoImagen.DetectarAsync(comando.Contenido, ct)
                   ?? throw new ValidationException([new ValidationFailure("archivo", SubirFotografiaValidator.MensajeFormato)]);

        var nombre = $"{Guid.NewGuid():N}{tipo.Extension}";
        var ruta = $"usuario-{idUsuario}/cultivo-{idCultivo}/{nombre}";
        await almacen.SubirAsync(ruta, comando.Contenido, tipo.Tipo, ct);

        FotografiaRegistrada registro;
        try
        {
            registro = await procedimientos.RegistrarAsync(idUsuario, idCultivo, new NuevaFotografia(
                comando.IdAvance, comando.IdLocal, ruta, nombre, (int)Math.Ceiling(comando.TamanoBytes / 1024.0),
                comando.EsPortada, comando.Latitud, comando.Longitud), ct);
        }
        catch
        {
            await almacen.EliminarAsync(ruta, ct);   // 404, 409 o 400 de la BD: no se deja el archivo suelto
            throw;
        }

        if (registro.YaExistia)
            await almacen.EliminarAsync(ruta, ct);   // la foto ya estaba (mismo idLocal): sobra la copia nueva

        var datos = (await procedimientos.ObtenerAsync(registro.IdFotografia, idUsuario, ct))!;
        var dto = new FotografiaSubidaDto(datos.Id, datos.IdLocal, datos.IdCultivo, datos.IdAvance,
            almacen.ObtenerUrl(datos.Ruta), datos.NombreArchivo, datos.TamanoKb, datos.EsPortada,
            datos.Latitud, datos.Longitud, datos.FechaCaptura);
        return (dto, !registro.YaExistia);
    }
}