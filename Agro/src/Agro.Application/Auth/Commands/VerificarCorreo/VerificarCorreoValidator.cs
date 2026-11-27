using Agro.Application.Auth.Commands.RestablecerContrasena;
using FluentValidation;

namespace Agro.Application.Auth.Commands.VerificarCorreo;

public sealed class VerificarCorreoValidator : AbstractValidator<VerificarCorreoCommand>
{
    public VerificarCorreoValidator()
    {
        RuleFor(x => x.Token).NotEmpty().WithMessage(RestablecerContrasenaHandler.EnlaceInvalido);
    }
}