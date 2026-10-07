using Agro.Api.Middleware;
using Agro.Application;
using Agro.Infrastructure;
using Agro.Infrastructure.Configuration;
using Agro.Infrastructure.Persistence;
using Scalar.AspNetCore;

// Carga el archivo .env (busca hacia arriba desde la carpeta de ejecución).
// NoClobber: si la variable ya existe en el sistema (por ejemplo en Azure), se respeta.
DotNetEnv.Env.NoClobber().TraversePath().Load();

// Configuración obligatoria: si falta una variable del .env, se avisa claro y no arranca.
string cadenaConexion;
string[] origenesFrontend;
try
{
    cadenaConexion = ConfiguracionBaseDatos.CadenaConexion();
    origenesFrontend = VariablesEntorno.OrigenesFrontend();
}
catch (InvalidOperationException ex)
{
    Console.Error.WriteLine($"Error de configuración: {ex.Message}");
    return 1;
}

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------
// Servicios
// ---------------------------------------------------------------------

builder.Services.AddApplication();                          // commands, queries y validadores
builder.Services.AddInfraestructure(cadenaConexion);        // PostgreSQL (agro_db)

// CORS: solo se aceptan peticiones del dominio del frontend
const string PoliticaFrontend = "Frontend";
builder.Services.AddCors(opciones => opciones.AddPolicy(PoliticaFrontend, politica =>
    politica.WithOrigins(origenesFrontend)
            .AllowAnyHeader()
            .AllowAnyMethod()));

// Todas las respuestas de error en JSON (ProblemDetails)
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManejadorExcepciones>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// ---------------------------------------------------------------------
// Canal de peticiones (el orden importa)
// ---------------------------------------------------------------------

app.UseExceptionHandler();   // excepciones -> JSON
app.UseStatusCodePages();    // 404, 405… sin cuerpo -> JSON

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();                 // /openapi/v1.json
    app.MapScalarApiReference();      // /scalar: probar la API desde el navegador
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
    if (conectado)
        app.Logger.LogInformation("{Mensaje}", mensaje);
    else
        app.Logger.LogError("{Mensaje}. La API sigue activa y /api/salud responderá 503.", mensaje);
}

app.Run();
return 0;
