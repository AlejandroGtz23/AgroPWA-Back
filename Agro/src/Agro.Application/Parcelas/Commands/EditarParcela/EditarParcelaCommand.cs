using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Parcelas.Commands.EditarParcela;

public sealed record EditarParcelaCommand(string Nombre, decimal SuperficieHa, string? UbicacionReferencia);
