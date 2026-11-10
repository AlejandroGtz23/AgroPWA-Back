using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Avances.Commands.RegistrarAvance;

public sealed class RegistrarAvanceValidator : AbstractValidator<RegistrarAvanceCommand>
{
    public RegistrarAvanceValidator(TimeProvider reloj)
    {
        RuleFor(x => x.IdTipoActividad).NotNull().WithMessage("El tipo de actividad es obligatorio.");

        RuleFor(x => x.FechaActividad)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("La fecha es obligatoria.")
            // 5 minutos de tolerancia por si el reloj del teléfono va adelantado
            .Must(f => f <= reloj.GetUtcNow().AddMinutes(5)).WithMessage("La fecha no puede estar en el futuro.");

        RuleFor(x => x.Descripcion)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("La descripción es obligatoria.")
            .MaximumLength(500).WithMessage("La descripción no puede tener más de 500 caracteres.");

        // La BD exige duración y método si es riego, y producto, cantidad y unidad si es fertilización
        RuleFor(x => x.DuracionMin).GreaterThan(0).WithMessage("La duración debe ser mayor que 0 minutos.");
        RuleFor(x => x.MetodoRiego).MaximumLength(30).WithMessage("El método de riego no puede tener más de 30 caracteres.");
        RuleFor(x => x.Producto).MaximumLength(100).WithMessage("El producto no puede tener más de 100 caracteres.");
        RuleFor(x => x.Cantidad)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(0).WithMessage("La cantidad debe ser mayor que 0.")
            .PrecisionScale(10, 2, true).WithMessage("La cantidad admite máximo 2 decimales.");
        RuleFor(x => x.Unidad).MaximumLength(20).WithMessage("La unidad no puede tener más de 20 caracteres.");
    }
}