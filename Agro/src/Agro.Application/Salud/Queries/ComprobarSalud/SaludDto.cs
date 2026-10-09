namespace Agro.Application.Salud.Queries.ComprobarSalud;

/// <summary>Respuesta de GET /api/salud. Estado: "OK" o "ERROR".</summary>
public record SaludDto(string Estado, string BaseDatos, DateTime Fecha, string? Mensaje = null, string Saludo = "hola mundo");
