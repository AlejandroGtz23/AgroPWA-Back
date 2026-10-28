using Agro.Application.Parcelas.Common;
using FluentValidation;

namespace Agro.Application.Parcelas.Commands.EditarParcela;

public sealed class EditarParcelaValidator : AbstractValidator<EditarParcelaCommand>
{
    public EditarParcelaValidator()
    {
        RuleFor(x => x.Nombre).NombreParcela();
        RuleFor(x => x.SuperficieHa).Superficie();
        RuleFor(x => x.UbicacionReferencia).Ubicacion();
    }
}
