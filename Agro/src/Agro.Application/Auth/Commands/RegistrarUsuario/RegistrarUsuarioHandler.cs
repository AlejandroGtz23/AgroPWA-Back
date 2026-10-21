using Agro.Application.Common.Exceptions;
using Agro.Application.Common.Interfaces;
using Agro.Application.Common.Models;
using Agro.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Agro.Application.Auth.Commands.RegistrarUsuario;

/// <summary>Command: crea la cuenta de un productor (HU-09, criterios 1 y 2).</summary>
public sealed class RegistrarUsuarioHandler(
    IAuthProcedimientos procedimientos,
    IHashContrasena hashContrasena,
    IValidator<RegistrarUsuarioCommand> validador)
{
    private const string RolPorDefecto = "productor";

    public async Task<UsuarioRegistradoDto> EjecutarAsync(RegistrarUsuarioCommand comando, CancellationToken ct = default)
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

        return new UsuarioRegistradoDto(idUsuario, datos.Nombre, datos.Correo, RolPorDefecto);
    }

}