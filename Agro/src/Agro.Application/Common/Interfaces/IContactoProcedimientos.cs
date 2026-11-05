using System;
using System.Collections.Generic;
using System.Text;
using Agro.Application.Common.Models;

namespace Agro.Application.Common.Interfaces;

public interface IContactoProcedimientos
{
    Task<bool> RegistrarAsync(NuevoMensajeContacto mensaje, int maximoPorHora, CancellationToken ct = default);
}
