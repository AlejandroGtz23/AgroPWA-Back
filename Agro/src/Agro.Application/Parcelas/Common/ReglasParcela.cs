using Agro.Application.Common.Models;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Parcelas.Common;

public static class ReglasParcela
{
    public const decimal SuperficieMaxima = 999_999.99m;   // NUMERIC(8,2)

    public static IRuleBuilderOptions<T, string> NombreParcela<T>(this IRuleBuilderInitial<T, string> regla) =>
        regla.Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("El nombre de la parcela es obligatorio.")
            .MaximumLength(80).WithMessage("El nombre no puede tener más de 80 caracteres.");

    public static IRuleBuilderOptions<T, decimal> Superficie<T>(this IRuleBuilderInitial<T, decimal> regla) =>
        regla.Cascade(CascadeMode.Stop)
            .GreaterThan(0).WithMessage("Ingresa un número válido.")
            .LessThanOrEqualTo(SuperficieMaxima).WithMessage("La superficie no puede ser mayor que 999,999.99 ha.")
            .PrecisionScale(8, 2, true).WithMessage("La superficie puede tener máximo 2 decimales.");

    public static IRuleBuilderOptions<T, string?> Ubicacion<T>(this IRuleBuilderInitial<T, string?> regla) =>
        regla.MaximumLength(150).WithMessage("La ubicación no puede tener más de 150 caracteres.");

    /// <summary>Quita espacios y convierte la ubicación vacía en null.</summary>
    public static DatosParcela Normalizar(string nombre, decimal superficieHa, string? ubicacion) =>
        new(nombre.Trim(), superficieHa, string.IsNullOrWhiteSpace(ubicacion) ? null : ubicacion.Trim());
}
