
using FluentValidation;

namespace Agro.Application.Auth.Commands.RegistrarUsuario;
/// <summary>
/// Coinsiden los check con la tabla usuarios
/// </summary>
public sealed class RegistrarUsuarioValidator : AbstractValidator<RegistrarUsuarioCommand>
{
    public RegistrarUsuarioValidator()
    {
        RuleFor(x => x.Nombre)
            .Cascade(CascadeMode.Stop)   // si falta el dato, no evalúa las demás reglas (evita null.Trim())
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .Must(n => n.Trim().Length >= 2).WithMessage("El nombre debe tener al menos 2 letras.")
            .MaximumLength(60);

        RuleFor(x => x.ApellidoPaterno)
            .Cascade(CascadeMode.Stop)   // si falta el dato, no evalúa las demás reglas (evita null.Trim())
            .NotEmpty().WithMessage("El apellido paterno es obligatorio.")
            .Must(a => a.Trim().Length >= 2).WithMessage("El apellido paterno debe tener al menos 2 letras.")
            .MaximumLength(60);

        RuleFor(x => x.ApellidoMaterno).MaximumLength(60);

        RuleFor(x => x.Correo)
            .NotEmpty().WithMessage("El correo es obligatorio.")
            .EmailAddress().WithMessage("Escribe un correo válido (ej. nombre@correo.com).")
            .MaximumLength(150);

        RuleFor(x => x.Telefono)
            .Matches(@"^[0-9]{10}$").WithMessage("El teléfono debe tener 10 dígitos.")
            .When(x => !string.IsNullOrWhiteSpace(x.Telefono));   // es opcional (migración 004)

        RuleFor(x => x.FechaNacimiento)
            .GreaterThanOrEqualTo(new DateOnly(1900, 1, 1)).WithMessage("La fecha de nacimiento no es válida.")
            .Must(f => f < DateOnly.FromDateTime(DateTime.UtcNow)).WithMessage("La fecha de nacimiento no puede ser futura.");

        RuleFor(x => x.Contrasena)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .MaximumLength(72).WithMessage("La contraseña no puede tener más de 72 caracteres.");

        RuleFor(x => x.AceptaTerminos)
            .Equal(true).WithMessage("Debes aceptar los términos y el aviso de privacidad.");
    }
}
