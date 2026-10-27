using Agro.Application.Common.Interfaces;
using Agro.Application.Parcelas.Common;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Parcelas.Commands.RegistrarParcela;
/// <summary>Command: registra una parcela del usuario (HU-13, criterio 1).
/// Si el idLocal ya existe (el dispositivo reenvió la parcela), devuelve la existente.</summary>
public sealed class RegistrarParcelaHandler(
    IParcelasProcedimientos procedimientos,
    IValidator<RegistrarParcelaCommand> validador)
{
    public async Task<(ParcelaDto Parcela, bool Creada)> EjecutarAsync(int idUsuario,
        RegistrarParcelaCommand comando, CancellationToken ct = default)
    {
        await validador.ValidateAndThrowAsync(comando, ct);                       // 400

        var datos = ReglasParcela.Normalizar(comando.Nombre, comando.SuperficieHa, comando.UbicacionReferencia);
        var registro = await procedimientos.RegistrarAsync(idUsuario, comando.IdLocal, datos, ct);   // 409 si el nombre se repite

        var parcelas = await procedimientos.ObtenerAsync(idUsuario, registro.IdParcela, ct);
        return (parcelas[0], !registro.YaExistia);
    }
}