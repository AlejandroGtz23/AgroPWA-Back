using FluentValidation;

namespace Agro.Application.Auth.Commands.SolicitarRecuperacion;

public sealed class SolicitarRecuperacionValidator : AbstractValidator<SolicitarRecuperacionCommand>
{
    public SolicitarRecuperacionValidator()
    {
        RuleFor(x => x.Correo)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("El correo es obligatorio.")
            .EmailAddress().WithMessage("Escribe un correo válido (ej. nombre@correo.com).")
            .MaximumLength(150);
    }
}