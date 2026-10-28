using Agro.Application.Auth.Commands.CerrarSesion;
using Agro.Application.Auth.Commands.IniciarSesion;
using Agro.Application.Auth.Commands.RegistrarUsuario;
using Agro.Application.Auth.Commands.RenovarSesion;
using Agro.Application.Auth.Common;
using Agro.Application.Dashboard.Queries.ObtenerResumenDashboard;
using Agro.Application.Parcelas.Commands.EditarParcela;
using Agro.Application.Parcelas.Commands.RegistrarParcela;
using Agro.Application.Parcelas.Queries.ObtenerMisParcelas;
using Agro.Application.Salud.Queries.ComprobarSalud;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Agro.Application;

/// <summary>Registra los commands, queries y validadores de Application.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ValidatorOptions.Global.PropertyNameResolver = (_, miembro, _) =>
            miembro is null ? null : 
            System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(miembro.Name);

        // Validadores de FluentValidation de todo el proyecto (se irán agregando por historia)
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Queries
        services.AddScoped<ComprobarSaludHandler>();

        // Comunes de autenticación
        services.AddScoped<CreadorSesion>();
        services.AddScoped<ValidadorSesion>();
        // Commands
        services.AddScoped<RegistrarUsuarioHandler>();
        services.AddScoped<IniciarSesionHandler>();
        services.AddScoped<RenovarSesionHandler>();
        services.AddScoped<CerrarSesionHandler>();

        // Dashboard (HU-12)
        services.AddScoped<ObtenerResumenDashboardHandler>();

        // Parcelas (HU-13)
        services.AddScoped<RegistrarParcelaHandler>();
        services.AddScoped<EditarParcelaHandler>();
        services.AddScoped<ObtenerMisParcelasHandler>();

        return services;
    }
}
