using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Common.Models;

public sealed record CultivoEditado(NuevoCultivo Datos, string Estado, DateOnly? FechaCosecha,
    DateTimeOffset? FechaModificacion);