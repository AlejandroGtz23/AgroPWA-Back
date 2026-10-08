namespace Agro.Infrastructure.Configuration;

/// <summary>Lectura de variables de entorno (archivo .env en local, configuración del servidor en Azure).</summary>
public static class VariablesEntorno
{
    /// <summary>Devuelve la variable o lanza un error claro si falta.</summary>
    public static string Obtener(string nombre) =>
        Environment.GetEnvironmentVariable(nombre) is { Length: > 0 } valor
            ? valor
            : throw new InvalidOperationException(
                $"Falta la variable de entorno {nombre}. Copia .env.example como .env y complétalo.");

    /// <summary>Orígenes del frontend permitidos por CORS (FRONTEND_URL, separados por coma).</summary>
    public static string[] OrigenesFrontend() =>
        Obtener("FRONTEND_URL").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
