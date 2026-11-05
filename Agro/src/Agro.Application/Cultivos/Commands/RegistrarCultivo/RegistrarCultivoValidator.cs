using Agro.Application.Cultivos.Common;
using FluentValidation;

namespace Agro.Application.Cultivos.Commands.RegistrarCultivo;

public sealed class RegistrarCultivoValidator : AbstractValidator<RegistrarCultivoCommand>
{
    public RegistrarCultivoValidator(TimeProvider reloj)
    {
        RuleFor(x => x.Nombre).NombreCultivo();
        RuleFor(x => x.IdTipoCultivo).NotNull().WithMessage("El tipo de cultivo es obligatorio.");
        RuleFor(x => x.IdParcela).NotNull().WithMessage("La parcela es obligatoria.");
        RuleFor(x => x.FechaSiembra).FechaSiembra(reloj);
        RuleFor(x => x.Variedad).Variedad();
        RuleFor(x => x.Observaciones).Observaciones();
    }
}
