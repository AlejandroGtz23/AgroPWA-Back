using Agro.Application.Common.Exceptions;
using Agro.Application.Common.Interfaces;
using Agro.Application.Perfil.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Perfil.Queries.ObtenerPerfil;

public sealed class ObtenerPerfilHandler(IPerfilProcedimientos procedimientos)
{
    public const string NoEncontrado = "Usuario no encontrado.";

    public async Task<PerfilDto> EjecutarAsync(int idUsuario, CancellationToken ct = default) =>
        await procedimientos.ObtenerAsync(idUsuario, ct)
        ?? throw new NoEncontradoException(NoEncontrado);
}
