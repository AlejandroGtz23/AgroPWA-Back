using Agro.Application.Common.Interfaces;
using Agro.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Agro.Infrastructure;

/// <summary>Registra todo lo de Infrastructure con una sola línea en Program.cs.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfraestructure(this IServiceCollection services, string cadenaConexion)
    {
        // Base de datos agro_db (PostgreSQL)
        services.AddDbContext<AgroDbContext>(opciones => opciones.UseNpgsql(cadenaConexion));

        // Cuando Application pida IAgroDbContext, se le entrega el mismo AgroDbContext de la petición
        services.AddScoped<IAgroDbContext>(sp => sp.GetRequiredService<AgroDbContext>());

        return services;
    }
}
