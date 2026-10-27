using Agro.Application.Common.Interfaces;
using Agro.Infrastructure.Configuration;
using Agro.Infrastructure.Persistence;
using Agro.Infrastructure.Persistence.Procedimientos;
using Agro.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Agro.Infrastructure;

/// <summary>Registra todo lo de Infrastructure con una sola línea en Program.cs.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfraestructure(this IServiceCollection services, string cadenaConexion, ConfiguracionJwt configuracionJwt)
    {
        // Base de datos 
        services.AddDbContext<AgroDbContext>(opciones => opciones.UseNpgsql(cadenaConexion));
        services.AddScoped<IAgroDbContext>(sp => sp.GetRequiredService<AgroDbContext>());
        services.AddScoped<IAuthProcedimientos, AuthProcedimientos>();
        services.AddScoped<IParcelasProcedimientos, ParcelasProcedimientos>();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton(configuracionJwt);
        services.AddSingleton<IHashContrasena, HashContrasenaBCrypt>();
        services.AddSingleton<IGeneradorJwt, GeneradorJwt>();
        return services;
    }
}
