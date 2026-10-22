
namespace Agro.Application.Auth.Commands.RegistrarUsuario;
/// <summary>
/// Datos para crear una cuenta
/// </summary>
public sealed record RegistrarUsuarioCommand(
    string Nombre,
    string ApellidoPaterno,
    string? ApellidoMaterno,
    string Correo,
    string? Telefono,
    DateOnly FechaNacimiento,
    string Contrasena,
    bool AceptaTerminos
    );

