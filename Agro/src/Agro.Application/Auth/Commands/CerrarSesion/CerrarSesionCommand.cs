namespace Agro.Application.Auth.Commands.CerrarSesion;

/// <summary>Cerrar la sesión del token actual (HU-11). El jti sale del token, no del usuario.</summary>
public sealed record CerrarSesionCommand(string Jti);