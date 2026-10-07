using Agro.Application.Salud.Queries.ComprobarSalud;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Agro.Application;

/// <summary>Registra los commands, queries y validadores de Application.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Validadores de FluentValidation de todo el proyecto (se irán agregando por historia)
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Queries
        services.AddScoped<ComprobarSaludHandler>();

        // Commands (HU-09 en adelante): services.AddScoped<RegistrarUsuarioHandler>(); …

        return services;
    }
}
