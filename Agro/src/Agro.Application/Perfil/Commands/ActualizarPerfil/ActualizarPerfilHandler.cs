using Agro.Application.Common.Exceptions;
using Agro.Application.Common.Interfaces;
using Agro.Application.Common.Models;
using Agro.Application.Perfil.Common;
using FluentValidation;
using Agro.Application.Perfil.Queries.ObtenerPerfil;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Perfil.Commands.ActualizarPerfil;

public sealed class ActualizarPerfilHandler(
    IPerfilProcedimientos procedimientos,
    IValidator<ActualizarPerfilCommand> validador)
{
    public async Task<PerfilDto> EjecutarAsync(int idUsuario, ActualizarPerfilCommand comando, CancellationToken ct = default)
    {
        await validador.ValidateAndThrowAsync(comando, ct);                                 // 400

        var datos = new DatosPerfil(comando.Nombre, comando.ApellidoPaterno, comando.ApellidoMaterno,
            comando.Telefono, comando.ComunidadMunicipio);

        if (!await procedimientos.ActualizarAsync(idUsuario, datos, ct))
            throw new NoEncontradoException(ObtenerPerfilHandler.NoEncontrado);             // 404

        return (await procedimientos.ObtenerAsync(idUsuario, ct))!;
    }
}