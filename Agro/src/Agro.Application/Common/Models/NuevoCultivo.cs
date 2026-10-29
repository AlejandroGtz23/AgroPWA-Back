using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Common.Models;
public sealed record NuevoCultivo(
    int IdParcela, int IdTipoCultivo, string Nombre, string? Variedad,
    DateOnly FechaSiembra, string? Observaciones);
