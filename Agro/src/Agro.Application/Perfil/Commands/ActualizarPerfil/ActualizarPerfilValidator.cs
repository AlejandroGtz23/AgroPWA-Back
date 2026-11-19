using FluentValidation;
using Agro.Application.Auth.Common;


namespace Agro.Application.Perfil.Commands.ActualizarPerfil;

public sealed class ActualizarPerfilValidator : AbstractValidator<ActualizarPerfilCommand>
{
    public ActualizarPerfilValidator()
    {
        RuleFor(x => x.Nombre).NombrePersona("nombre");
        RuleFor(x => x.ApellidoPaterno).NombrePersona("apellido paterno");
        RuleFor(x => x.ApellidoMaterno).MaximumLength(60);
        RuleFor(x => x.Telefono).Telefono();
        RuleFor(x => x.ComunidadMunicipio).MaximumLength(120)
            .WithMessage("La comunidad o municipio no puede tener más de 120 caracteres.");
    }
}