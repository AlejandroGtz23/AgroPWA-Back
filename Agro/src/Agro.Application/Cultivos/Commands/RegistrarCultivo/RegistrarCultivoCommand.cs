using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Cultivos.Commands.RegistrarCultivo;
public sealed record RegistrarCultivoCommand(Guid? IdLocal, string Nombre, int? IdTipoCultivo, string? Variedad,
    DateOnly? FechaSiembra, int? IdParcela, string? Observaciones);
