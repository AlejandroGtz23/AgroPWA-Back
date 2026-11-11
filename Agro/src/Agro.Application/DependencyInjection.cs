using Agro.Application.Auth.Commands.CerrarSesion;
using Agro.Application.Auth.Commands.IniciarSesion;
using Agro.Application.Auth.Commands.RegistrarUsuario;
using Agro.Application.Auth.Commands.RenovarSesion;
using Agro.Application.Auth.Common;
using Agro.Application.Avances.Commands.RegistrarAvance;
using Agro.Application.Catalogos.Queries.ObtenerTiposActividad;
using Agro.Application.Catalogos.Queries.ObtenerTiposCultivo;
using Agro.Application.Contacto.Commands.EnviarMensajeContacto;
using Agro.Application.Cultivos.Commands.EditarCultivo;
using Agro.Application.Cultivos.Commands.EliminarCultivo;
using Agro.Application.Cultivos.Commands.RegistrarCultivo;
using Agro.Application.Cultivos.Queries.ObtenerDetalleCultivo;
using Agro.Application.Cultivos.Queries.ObtenerMisCultivos;
using Agro.Application.Dashboard.Queries.ObtenerResumenDashboard;
using Agro.Application.Fotografias.Commands.EliminarFotografia;
using Agro.Application.Fotografias.Commands.SubirFotografia;
using Agro.Application.Fotografias.Common;
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

        // Dashboard
        services.AddScoped<ObtenerResumenDashboardHandler>();

        // Parcelas
        services.AddScoped<RegistrarParcelaHandler>();
        services.AddScoped<EditarParcelaHandler>();
        services.AddScoped<ObtenerMisParcelasHandler>();

        // Cultivos 
        services.AddScoped<ObtenerTiposCultivoHandler>();
        services.AddScoped<RegistrarCultivoHandler>();
        services.AddScoped<ObtenerMisCultivosHandler>();
        services.AddScoped<ObtenerDetalleCultivoHandler>();
        services.AddScoped<EditarCultivoHandler>();
        services.AddScoped<EliminarCultivoHandler>();

        // Contacto
        services.AddScoped<EnviarMensajeContactoHandler>();

        // Avances
        services.AddScoped<ObtenerTiposActividadHandler>();
        services.AddScoped<RegistrarAvanceHandler>();

        // Fotografías
        services.AddScoped<EnlacesFotografias>();
        services.AddScoped<SubirFotografiaHandler>();
        services.AddScoped<EliminarFotografiaHandler>();

        return services;
    }
}
