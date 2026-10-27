using Agro.Application.Parcelas.Common;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Parcelas.Commands.RegistrarParcela;

public sealed class RegistrarParcelaValidator : AbstractValidator<RegistrarParcelaCommand>
{
    public RegistrarParcelaValidator()
    {
        RuleFor(x => x.Nombre).NombreParcela();
        RuleFor(x => x.SuperficieHa).Superficie();
        RuleFor(x => x.UbicacionReferencia).Ubicacion();
    }
}