namespace Agro.Application.Auth.Common;
public sealed record SesionDto(string AccessToken, string RefreshToken, DateTime ExpiraUtc, UsuarioSesionDto Usuario);
