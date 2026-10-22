namespace Agro.Application.Common.Exceptions;
/// <summary>
/// Si el dato ya existe choca con otro 409 
/// el mensaje semuestra tal cual al usuario
/// </summary>
public sealed class ConflictoException(string mensaje) : Exception(mensaje);