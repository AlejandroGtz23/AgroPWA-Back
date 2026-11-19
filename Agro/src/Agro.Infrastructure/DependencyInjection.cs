using Agro.Application.Common.Interfaces;
using Agro.Infrastructure.Configuration;
using Agro.Infrastructure.Correo;
using Agro.Infrastructure.Persistence;
using Agro.Infrastructure.Persistence.Procedimientos;
using Agro.Infrastructure.Security;
using Agro.Infrastructure.Storage;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Agro.Infrastructure;

/// <summary>Registra todo lo de Infrastructure con una sola línea en Program.cs.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfraestructure(this IServiceCollection services, string cadenaConexion, 
        ConfiguracionJwt configuracionJwt, ConfiguracionAlmacen configuracionAlmacen, ConfiguracionCorreo configuracionCorreo)
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
        services.AddScoped<IDashboardProcedimientos, DashboardProcedimientos>();
        services.AddScoped<ICultivosProcedimientos, CultivosProcedimientos>();
        services.AddScoped<IContactoProcedimientos, ContactoProcedimientos>();
        services.AddScoped<IAvancesProcedimientos, AvancesProcedimientos>();

        // Almacenamiento de fotos
        services.AddSingleton(new BlobContainerClient(configuracionAlmacen.Conexion, configuracionAlmacen.Contenedor));
        services.AddSingleton<IAlmacenFotos, AlmacenFotosAzure>();
        services.AddScoped<IFotografiasProcedimientos, FotografiasProcedimientos>();
        services.AddScoped<ISincronizacionProcedimientos, SincronizacionProcedimientos>();

        services.AddScoped<IPerfilProcedimientos, PerfilProcedimientos>();

        // Correo: Gmail (SMTP) si hay SMTP_HOST; si no, la consola
        services.AddSingleton(configuracionCorreo);
        if (configuracionCorreo.UsaSmtp)
            services.AddSingleton<IEnvioCorreo, EnvioCorreoSmtp>();
        else
            services.AddSingleton<IEnvioCorreo, EnvioCorreoConsola>();

        services.AddScoped<IRecuperacionProcedimientos, RecuperacionProcedimientos>();


        return services;
    }
}
