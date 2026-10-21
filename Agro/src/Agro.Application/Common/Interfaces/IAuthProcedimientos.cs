
using Agro.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Common.Interfaces;
/// <summary>
/// Operaciones que se ejecutan con prodedimientos almacenados
/// </summary>
public interface IAuthProcedimientos
{
    Task<int> RegistrarUsuarioAsync(NuevoUsuario datos, CancellationToken ct = default);
}
