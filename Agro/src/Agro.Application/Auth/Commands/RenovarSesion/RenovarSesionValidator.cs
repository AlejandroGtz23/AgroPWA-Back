using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Auth.Commands.RenovarSesion;

public sealed class RenovarSesionValidator : AbstractValidator<RenovarSesionCommand>
{
    public RenovarSesionValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("El refresh token es obligatorio.");
    }
}
