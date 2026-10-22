namespace Agro.Application.Common.Models;
/// <summary> SE guarda el token de acceso y cuando se acaba o vence/// </summary>

public sealed record TokenGenerado(string Token, DateTime ExpiraUtc);