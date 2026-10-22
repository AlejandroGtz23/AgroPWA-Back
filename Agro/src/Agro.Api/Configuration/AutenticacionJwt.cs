using System.Text;
using Agro.Application.Auth.Common;
using Agro.Infrastructure.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Agro.Api.Configuration;

/// <summary>Autenticación con JWT: valida el token y que su sesión siga abierta.</summary>
public static class AutenticacionJwt
{
    public static IServiceCollection AddAutenticacionJwt(this IServiceCollection services, ConfiguracionJwt configuracion)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opciones =>
            {
                // Los claims conservan su nombre original: sub, email, role, jti
                opciones.MapInboundClaims = false;

                opciones.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = configuracion.Emisor,
                    ValidateAudience = true,
                    ValidAudience = configuracion.Audiencia,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuracion.Clave)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),   // tolerancia por diferencias de reloj
                    NameClaimType = JwtRegisteredClaimNames.Sub,
                    RoleClaimType = "role"
                };

                // Token válido → además, la sesión debe seguir abierta en la base (logout / inactividad)
                opciones.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async contexto =>
                    {
                        var jti = contexto.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                        var validador = contexto.HttpContext.RequestServices.GetRequiredService<ValidadorSesion>();

                        if (jti is null || !await validador.EsValidaAsync(jti, contexto.HttpContext.RequestAborted))
                            contexto.Fail("La sesión está cerrada o inactiva.");
                    }
                };
            });

        services.AddAuthorization();
        return services;
    }
}