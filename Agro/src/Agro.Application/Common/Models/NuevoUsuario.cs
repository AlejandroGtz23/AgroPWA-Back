namespace Agro.Application.Common.Models;
/// <summary>
/// Datos ya validos, listos para guardarse
/// </summary>
public sealed record NuevoUsuario(
    string Nombre, string ApellidoPaterno, string? ApellidoMaterno,
    string Correo, string? Telefono, DateOnly FechaNacimiento, string ContrasenaHash);
