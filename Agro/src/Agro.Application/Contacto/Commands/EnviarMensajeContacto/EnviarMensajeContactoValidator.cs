using FluentValidation;

namespace Agro.Application.Contacto.Commands.EnviarMensajeContacto;

public sealed class EnviarMensajeContactoValidator : AbstractValidator<EnviarMensajeContactoCommand>
{
    public EnviarMensajeContactoValidator()
    {
        RuleFor(x => x.Nombre)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(120).WithMessage("El nombre no puede tener más de 120 caracteres.");

        RuleFor(x => x.Correo)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("El correo es obligatorio.")
            .MaximumLength(150).WithMessage("El correo no puede tener más de 150 caracteres.")
            .EmailAddress().WithMessage("Escribe un correo válido (ej. nombre@correo.com).")
            // Igual que el CHECK de la tabla: exige un punto después de la @
            .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$").WithMessage("Escribe un correo válido (ej. nombre@correo.com).");

        RuleFor(x => x.Asunto)
            .MaximumLength(150).WithMessage("El asunto no puede tener más de 150 caracteres.");

        RuleFor(x => x.Mensaje)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Escribe tu mensaje.")
            .MaximumLength(1000).WithMessage("El mensaje admite hasta 1000 caracteres.");
    }
}