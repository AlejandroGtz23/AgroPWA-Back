using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Cultivos.Commands.EditarCultivo;

public sealed record EditarCultivoCommand(string Nombre, int? IdTipoCultivo, string? Variedad,
    DateOnly? FechaSiembra, int? IdParcela, string? Estado, DateOnly? FechaCosecha,
    string? Observaciones, DateTimeOffset? FechaModificacion);