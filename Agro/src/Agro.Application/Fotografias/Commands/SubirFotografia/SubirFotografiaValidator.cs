using FluentValidation;

namespace Agro.Application.Fotografias.Commands.SubirFotografia;

public sealed class SubirFotografiaValidator : AbstractValidator<SubirFotografiaCommand>
{
    public const long TamanoMaximo = 5 * 1024 * 1024;   // 5 MB (CHECK de la tabla: 5120 KB)
    public const string MensajeFormato = "Solo se aceptan imágenes JPG o PNG de hasta 5 MB.";

    public SubirFotografiaValidator()
    {
        // El error sale en «archivo», que es como se llama el campo en el formulario
        RuleFor(x => x.TamanoBytes)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(0).WithMessage("Selecciona una imagen.")
            .LessThanOrEqualTo(TamanoMaximo).WithMessage(MensajeFormato)
            .OverridePropertyName("archivo");

        // Mismos rangos que los CHECK de la tabla fotografia
        RuleFor(x => x.Latitud)
            .InclusiveBetween(-90, 90).WithMessage("La latitud debe estar entre -90 y 90.");
        RuleFor(x => x.Longitud)
            .InclusiveBetween(-180, 180).WithMessage("La longitud debe estar entre -180 y 180.");
    }
}