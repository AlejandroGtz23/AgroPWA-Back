using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Common.Models;

public sealed record DatosParcela(string Nombre, decimal SuperficieHa, string? UbicacionReferencia);
