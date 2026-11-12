using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Sincronizacion.Commands.ActualizarPreferencias;

public sealed class ActualizarPreferenciasValidator : AbstractValidator<ActualizarPreferenciasCommand>
{
    public ActualizarPreferenciasValidator()
    {
        // NUMERIC(8,2) y CHECK >= 0 de la tabla dispositivo
        RuleFor(x => x.EspacioUsadoMb)
            .Cascade(CascadeMode.Stop)
            .GreaterThanOrEqualTo(0).WithMessage("El espacio usado no puede ser negativo.")
            .PrecisionScale(8, 2, true).WithMessage("El espacio usado admite máximo 6 enteros y 2 decimales.");
    }
}
