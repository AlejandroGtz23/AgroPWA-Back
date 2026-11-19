
using Agro.Application.Auth.Common;
using FluentValidation;

namespace Agro.Application.Auth.Commands.RegistrarUsuario;
/// <summary>
/// Coinsiden los check con la tabla usuarios
/// </summary>
public sealed class RegistrarUsuarioValidator : AbstractValidator<RegistrarUsuarioCommand>
{
    public RegistrarUsuarioValidator()
    {
        RuleFor(x => x.Nombre).NombrePersona("nombre");
        RuleFor(x => x.ApellidoPaterno).NombrePersona("apellido paterno");
        RuleFor(x => x.ApellidoMaterno).MaximumLength(60);

        RuleFor(x => x.Correo)
            .NotEmpty().WithMessage("El correo es obligatorio.")
            .EmailAddress().WithMessage("Escribe un correo válido (ej. nombre@correo.com).")
            .MaximumLength(150);

        RuleFor(x => x.Telefono).Telefono();

        RuleFor(x => x.FechaNacimiento)
            .GreaterThanOrEqualTo(new DateOnly(1900, 1, 1)).WithMessage("La fecha de nacimiento no es válida.")
            .Must(f => f < DateOnly.FromDateTime(DateTime.UtcNow)).WithMessage("La fecha de nacimiento no puede ser futura.");

        RuleFor(x => x.Contrasena).Contrasena();

        RuleFor(x => x.AceptaTerminos)
            .Equal(true).WithMessage("Debes aceptar los términos y el aviso de privacidad.");
    }
}
