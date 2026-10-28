using Agro.Application.Common.Interfaces;
using Agro.Application.Parcelas.Common;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Parcelas.Commands.RegistrarParcela;

public sealed record RegistrarParcelaCommand(Guid? IdLocal, string Nombre, decimal SuperficieHa, string? UbicacionReferencia);
