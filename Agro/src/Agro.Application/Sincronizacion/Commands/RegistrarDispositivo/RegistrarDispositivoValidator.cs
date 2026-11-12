using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Sincronizacion.Commands.RegistrarDispositivo;

public sealed class RegistrarDispositivoValidator : AbstractValidator<RegistrarDispositivoCommand>
{
    public RegistrarDispositivoValidator()
    {
        RuleFor(x => x.Identificador)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("El identificador del dispositivo es obligatorio.")
            .MaximumLength(64).WithMessage("El identificador no puede tener más de 64 caracteres.");

        RuleFor(x => x.Nombre)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("El nombre del dispositivo es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede tener más de 100 caracteres.");
    }
}