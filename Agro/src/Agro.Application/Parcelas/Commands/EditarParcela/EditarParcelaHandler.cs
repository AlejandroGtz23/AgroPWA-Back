using Agro.Application.Common.Exceptions;
using Agro.Application.Common.Interfaces;
using Agro.Application.Parcelas.Common;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Parcelas.Commands.EditarParcela;

public sealed class EditarParcelaHandler(
    IParcelasProcedimientos procedimientos,
    IValidator<EditarParcelaCommand> validador)
{
    public async Task<ParcelaDto> EjecutarAsync(int idParcela, int idUsuario,
        EditarParcelaCommand comando, CancellationToken ct = default)
    {
        await validador.ValidateAndThrowAsync(comando, ct);                           // 400

        var datos = ReglasParcela.Normalizar(comando.Nombre, comando.SuperficieHa, comando.UbicacionReferencia);
        if (!await procedimientos.EditarAsync(idParcela, idUsuario, datos, ct))       // 409 si el nombre se repite
            throw new NoEncontradoException("La parcela no existe.");                 // 404

        return (await procedimientos.ObtenerAsync(idUsuario, idParcela, ct))[0];
    }
}
