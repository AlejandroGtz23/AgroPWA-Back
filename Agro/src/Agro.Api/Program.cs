using Agro.Api.Configuration;
using Agro.Api.Middleware;
using Agro.Application;
using Agro.Infrastructure;
using Agro.Infrastructure.Configuration;
using Agro.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using System.Threading.RateLimiting;


// Carga el archivo .env (busca hacia arriba desde la carpeta de ejecución).
// NoClobber: si la variable ya existe en el sistema (por ejemplo en Azure), se respeta.
DotNetEnv.Env.NoClobber().TraversePath().Load();

// Configuración obligatoria: si falta una variable del .env, se avisa claro y no arranca.
string cadenaConexion;
string[] origenesFrontend;
ConfiguracionJwt configuracionJwt;
try
{
    cadenaConexion = ConfiguracionBaseDatos.CadenaConexion();
    origenesFrontend = VariablesEntorno.OrigenesFrontend();
    configuracionJwt = ConfiguracionJwt.DesdeVariables();
}
catch (InvalidOperationException ex)
{
    await Console.Error.WriteLineAsync($"Error de configuración: {ex.Message}");
    return 1;
}

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------
// Servicios
// ---------------------------------------------------------------------

builder.Services.AddApplication();                          // commands, queries y validadores
builder.Services.AddInfraestructure(cadenaConexion, configuracionJwt);        // PostgreSQL (agro_db)

// CORS: solo se aceptan peticiones del dominio del frontend
const string PoliticaFrontend = "Frontend";
builder.Services.AddCors(opciones => opciones.AddPolicy(PoliticaFrontend, politica =>
    politica.WithOrigins(origenesFrontend)
            .AllowAnyHeader()
            .AllowAnyMethod()));

// Todas las respuestas de error en JSON (ProblemDetails)
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManejadorExcepciones>();

builder.Services.AddControllers(opciones =>
        opciones.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
    .ConfigureApiBehaviorOptions(opciones =>
        // JSON con un tipo incorrecto (letras en un número, fecha inválida): mismo formato que FluentValidation
        opciones.InvalidModelStateResponseFactory = contexto =>
        {
            var detalle = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Los datos enviados no son válidos."
            };
            detalle.Extensions["errores"] = contexto.ModelState
                .Where(campo => campo.Key.StartsWith('$') && campo.Value?.Errors.Count > 0)
                .ToDictionary(
                    campo => System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(campo.Key.TrimStart('$', '.')),
                    _ => new[] { "El valor no tiene el formato correcto." });

            return new BadRequestObjectResult(detalle) { ContentTypes = { "application/problem+json" } };
        });

builder.Services.AddDocumentacionApi();                      // OpenAPI + botón Authorize (HU-31)
builder.Services.AddAutenticacionJwt(configuracionJwt);      // JWT + validación de la sesión (HU-10)

// Límite de peticiones al login: máximo 10 por minuto por IP (protección contra fuerza bruta)
builder.Services.AddRateLimiter(opciones =>
{
    opciones.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    opciones.AddPolicy("login", contexto => RateLimitPartition.GetFixedWindowLimiter(
        contexto.Connection.RemoteIpAddress?.ToString() ?? "desconocida",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});

var app = builder.Build();

// ---------------------------------------------------------------------
// Canal de peticiones (el orden importa)
// ---------------------------------------------------------------------

app.UseExceptionHandler();   // excepciones -> JSON
app.UseStatusCodePages();    // 404, 405… sin cuerpo -> JSON

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();                 // /openapi/v1.json
    app.UseSwaggerUI(opciones => // /swagger: probar la API desde el navegador
    {
        opciones.SwaggerEndpoint("/openapi/v1.json", "Agro API v1");
        opciones.DocumentTitle = "Agro API · Swagger";
    });      
}

app.UseHttpsRedirection();
app.UseCors(PoliticaFrontend);
app.UseAuthentication();   // 1. ¿quién eres? (lee y valida el token)
app.UseAuthorization();    // 2. ¿puedes entrar? ([Authorize])
app.UseRateLimiter();
app.MapControllers();

// Criterios 2 y 4: probar la conexión al arrancar y registrarla en la consola,
// sin mostrar nunca la contraseña ni la cadena de conexión.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AgroDbContext>();
    var (conectado, mensaje) = await VerificacionConexion.ProbarAsync(db);
    if (!conectado)
        app.Logger.LogError("{Mensaje}. La API sigue activa y /api/salud responderá 503.", mensaje);
    else if (app.Logger.IsEnabled(LogLevel.Information))
        app.Logger.LogInformation("{Mensaje}", mensaje);
}

await app.RunAsync();
return 0;
