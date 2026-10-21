using Agro.Api.Middleware;
using Agro.Application;
using Agro.Infrastructure;
using Agro.Infrastructure.Configuration;
using Agro.Infrastructure.Persistence;


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
    opciones.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);

builder.Services.AddOpenApi(opciones => opciones.AddDocumentTransformer((documento, contexto, ct) =>
{
    documento.Info.Title = "Agro API";
    documento.Info.Version = "v1";
    documento.Info.Description = "API de Agro, la PWA para el registro y seguimiento de cultivos.";
    return Task.CompletedTask;
}));

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
app.UseAuthorization();
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
