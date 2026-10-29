using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agro.Application.Cultivos.Queries.ObtenerMisCultivos;
public sealed class ObtenerMisCultivosValidator : AbstractValidator<ObtenerMisCultivosQuery>
{
    public static readonly string[] Estados = ["activo", "finalizado", "perdido"];   // CHECK de la tabla cultivo
    public const int TamanoMaximo = 50;

    public ObtenerMisCultivosValidator()
    {
        RuleFor(x => x.Buscar).MaximumLength(80).WithMessage("La búsqueda no puede tener más de 80 caracteres.");

        RuleFor(x => x.Estado)
            .Must(e => string.IsNullOrWhiteSpace(e) || Estados.Contains(e.Trim().ToLowerInvariant()))
            .WithMessage("El estado debe ser activo, finalizado o perdido.");

        RuleFor(x => x.Pagina).GreaterThanOrEqualTo(1).WithMessage("La página debe ser 1 o mayor.");

        RuleFor(x => x.Tamano).InclusiveBetween(1, TamanoMaximo)
            .WithMessage($"El tamaño de página debe estar entre 1 y {TamanoMaximo}.");
    }
}