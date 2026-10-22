using Agro.Application.Auth.Common;
using Agro.Application.Common.Interfaces;
using Agro.Application.Common.Models;
using FluentValidation;


namespace Agro.Application.Auth.Commands.RegistrarUsuario;

/// <summary>Command: crea la cuenta de un productor (HU-09, criterios 1 y 2).</summary>
public sealed class RegistrarUsuarioHandler(
    IAuthProcedimientos procedimientos,
    IHashContrasena hashContrasena,
    CreadorSesion creadorSesion,
    IValidator<RegistrarUsuarioCommand> validador)
{
    private const string RolPorDefecto = "productor";

    public async Task<SesionDto> EjecutarAsync(RegistrarUsuarioCommand comando,
        string? ip, string? agenteUsuario, CancellationToken ct = default)
    {
        await validador.ValidateAndThrowAsync(comando, ct);                    // 400 si algo no es válido

        var datos = new NuevoUsuario(
            comando.Nombre.Trim(),
            comando.ApellidoPaterno.Trim(),
            string.IsNullOrWhiteSpace(comando.ApellidoMaterno) ? null : comando.ApellidoMaterno.Trim(),
            comando.Correo.Trim().ToLowerInvariant(),
            string.IsNullOrWhiteSpace(comando.Telefono) ? null : comando.Telefono.Trim(),
            comando.FechaNacimiento,
            hashContrasena.Generar(comando.Contrasena));                       // nunca en texto plano

        var idUsuario = await procedimientos.RegistrarUsuarioAsync(datos, ct); // 409 si el correo existe

        var usuario = new UsuarioSesionDto(idUsuario, datos.Nombre, datos.Correo, RolPorDefecto);
        return await creadorSesion.CrearAsync(usuario, recordarme: false, ip, agenteUsuario, ct);
    }

}