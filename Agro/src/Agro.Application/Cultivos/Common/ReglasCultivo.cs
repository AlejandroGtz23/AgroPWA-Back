using Agro.Application.Common;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Cultivos.Common;

public static class ReglasCultivo
{
    public static readonly string[] Estados = ["activo", "finalizado", "perdido"];
    public const string Finalizado = "finalizado";

    public static bool EsFinalizado(string? estado) => string.Equals(estado?.Trim(), Finalizado, StringComparison.OrdinalIgnoreCase);


    public static string? Limpiar(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();

    public static IRuleBuilderOptions<T, string> NombreCultivo<T>(this IRuleBuilderInitial<T, string> regla) =>
        regla.Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("El nombre del cultivo es obligatorio.")
            .MaximumLength(80).WithMessage("El nombre no puede tener más de 80 caracteres.");

    public static IRuleBuilderOptions<T, DateOnly?> FechaSiembra<T>(this IRuleBuilderInitial<T, DateOnly?> regla, TimeProvider reloj) =>
            regla.Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("La fecha de siembra es obligatoria.")
            .Must(f => f <= FechaMexico.Hoy(reloj)).WithMessage("La fecha de siembra no puede ser posterior a hoy.")
            .GreaterThanOrEqualTo(new DateOnly(2000, 1, 1)).WithMessage("La fecha de siembra no es válida.");

    public static IRuleBuilderOptions<T, string?> Variedad<T>(this IRuleBuilderInitial<T, string?> regla) =>
        regla.MaximumLength(80).WithMessage("La variedad no puede tener más de 80 caracteres.");

    public static IRuleBuilderOptions<T, string?> Observaciones<T>(this IRuleBuilderInitial<T, string?> regla) =>
        regla.MaximumLength(300).WithMessage("Las observaciones admiten hasta 300 caracteres.");
}
