using FluentValidation;
using Agro.Application.Auth.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Perfil.Commands.CambiarContrasena;

public sealed class CambiarContrasenaValidator : AbstractValidator<CambiarContrasenaCommand>
{
    public CambiarContrasenaValidator()
    {
        RuleFor(x => x.ContrasenaActual).NotEmpty().WithMessage("Escribe tu contraseña actual.");

        RuleFor(x => x.ContrasenaNueva)
            .Contrasena()
            .NotEqual(x => x.ContrasenaActual).WithMessage("La contraseña nueva debe ser diferente a la actual.");
    }
}