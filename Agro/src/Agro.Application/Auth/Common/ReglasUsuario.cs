using FluentValidation;
using System.Text.RegularExpressions;

namespace Agro.Application.Auth.Common;

/// <summary>Reglas del usuario que comparten el registro (HU-09) y el perfil (HU-26).</summary>
public static class ReglasUsuario
{
    /// <summary>Nombre o apellido obligatorio: 2 letras o más y máximo 60 (CHECK de la tabla usuario).</summary>
    public static IRuleBuilderOptions<T, string> NombrePersona<T>(this IRuleBuilderInitial<T, string> regla, string campo) =>
        regla.Cascade(CascadeMode.Stop)   // si falta el dato, no evalúa las demás reglas (evita null.Trim())
            .NotEmpty().WithMessage($"El {campo} es obligatorio.")
            .Must(n => n.Trim().Length >= 2).WithMessage($"El {campo} debe tener al menos 2 letras.")
            .MaximumLength(60);

    /// <summary>Teléfono opcional; si viene, 10 dígitos (migraciones 004 y 005).</summary>
    public static IRuleBuilderOptions<T, string?> Telefono<T>(this IRuleBuilderInitial<T, string?> regla) =>
        regla.Must(t => string.IsNullOrWhiteSpace(t) || (t.Length == 10 && t.All(char.IsAsciiDigit)))
            .WithMessage("El teléfono debe tener 10 dígitos.");

    /// <summary>Contraseña de 8 a 72 caracteres (72 es el límite de BCrypt).</summary>
    public static IRuleBuilderOptions<T, string> Contrasena<T>(this IRuleBuilderInitial<T, string> regla) =>
        regla.NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .MaximumLength(72).WithMessage("La contraseña no puede tener más de 72 caracteres.");
}