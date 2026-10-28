using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Parcelas.Common;

public sealed record ParcelaDto(int Id, Guid? IdLocal, string Nombre, decimal SuperficieHa, string? UbicacionReferencia, int NumeroCultivos);