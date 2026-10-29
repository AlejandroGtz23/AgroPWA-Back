using FluentValidation;
using Agro.Application.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Cultivos.Commands.RegistrarCultivo;

public sealed class RegistrarCultivoValidator : AbstractValidator<RegistrarCultivoCommand>
{
    public RegistrarCultivoValidator(TimeProvider reloj)
    {
        RuleFor(x => x.Nombre)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("El nombre del cultivo es obligatorio.")
            .MaximumLength(80).WithMessage("El nombre no puede tener más de 80 caracteres.");

        RuleFor(x => x.IdTipoCultivo)
            .NotNull().WithMessage("El tipo de cultivo es obligatorio.");

        RuleFor(x => x.IdParcela)
            .NotNull().WithMessage("La parcela es obligatoria.");

        RuleFor(x => x.FechaSiembra)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("La fecha de siembra es obligatoria.")
            .Must(f => f <= FechaMexico.Hoy(reloj)).WithMessage("La fecha de siembra no puede ser posterior a hoy.")
            .GreaterThanOrEqualTo(new DateOnly(2000, 1, 1)).WithMessage("La fecha de siembra no es válida.");

        RuleFor(x => x.Variedad).MaximumLength(80).WithMessage("La variedad no puede tener más de 80 caracteres.");
        RuleFor(x => x.Observaciones).MaximumLength(300).WithMessage("Las observaciones admiten hasta 300 caracteres.");
    }
}
