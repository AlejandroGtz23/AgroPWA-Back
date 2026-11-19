using Agro.Application.Auth.Common;
using FluentValidation;

namespace Agro.Application.Auth.Commands.RestablecerContrasena;

public sealed class RestablecerContrasenaValidator : AbstractValidator<RestablecerContrasenaCommand>
{
    public RestablecerContrasenaValidator()
    {
        RuleFor(x => x.Token).NotEmpty().WithMessage(RestablecerContrasenaHandler.EnlaceInvalido);
        RuleFor(x => x.ContrasenaNueva).Contrasena();
    }
}