using Agro.Application.Common;
using Agro.Application.Cultivos.Common;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Cultivos.Commands.EditarCultivo;

public sealed class EditarCultivoValidator : AbstractValidator<EditarCultivoCommand>
{
    public EditarCultivoValidator(TimeProvider reloj)
    {
        RuleFor(x => x.Nombre).NombreCultivo();
        RuleFor(x => x.IdTipoCultivo).NotNull().WithMessage("El tipo de cultivo es obligatorio.");
        RuleFor(x => x.IdParcela).NotNull().WithMessage("La parcela es obligatoria.");
        RuleFor(x => x.FechaSiembra).FechaSiembra(reloj);
        RuleFor(x => x.Variedad).Variedad();
        RuleFor(x => x.Observaciones).Observaciones();
        RuleFor(x => x.Estado)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("El estado es obligatorio.")
            .Must(e => ReglasCultivo.Estados.Contains(e!.Trim().ToLowerInvariant()))
            .WithMessage("El estado debe ser activo, finalizado o perdido.");

        When(x => ReglasCultivo.EsFinalizado(x.Estado), () =>
        {
            RuleFor(x => x.FechaCosecha)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Captura la fecha de cosecha.")
                .Must(f => f <= FechaMexico.Hoy(reloj)).WithMessage("La fecha de cosecha no puede ser posterior a hoy.")
                .Must((comando, f) => comando.FechaSiembra is null || f >= comando.FechaSiembra)
                .WithMessage("La fecha de cosecha no puede ser anterior a la de siembra.");
        });
    }
}
