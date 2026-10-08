# AgroPWA-Back
Proyecto especialmente dirigido para personas que trabajan directamente en el campo o cultivos.

API REST de **Agro**, la PWA para el registro y seguimiento de cultivos.
HU-04 · Configuración del entorno backend · Sprint 1
Equipo: Victor Manuel Jose Jose · Jesus Alejandro Gutierrez Montufar (10 IDGS-G3, UTTT)

## Tecnologías

| Herramienta | Versión |
|---|---|
| .NET / ASP.NET Core Web API (controladores) | 10.0 LTS |
| Entity Framework Core + Npgsql (PostgreSQL) | 10.x |
| PostgreSQL (base `agro_db`, HU-03) | 13 o superior (contenedor Docker) |
| FluentValidation | 12.x |
| BCrypt.Net-Next · JWT (Microsoft.IdentityModel) | para HU-09 y HU-10 |
| DotNetEnv (`.env`) · Scalar (documentación de la API) | 3.x · 2.x |
| xUnit (pruebas) | 2.x |

## Arquitectura: Clean Architecture + CQRS ligero

```
Api ──► Application ──► Domain
 └──► Infrastructure ──┘
```

| Proyecto | Responsabilidad |
|---|---|
| `Agro.Domain` | Entidades de `agro_db` (Usuario, Cultivo, Sesion…). No depende de nada |
| `Agro.Application` | Casos de uso por funcionalidad: **Commands** (escriben) y **Queries** (leen), validadores e interfaces (`IAgroDbContext`) |
| `Agro.Infrastructure` | Implementaciones técnicas: `AgroDbContext` (PostgreSQL), configuración del `.env`, seguridad |
| `Agro.Api` | Controladores, manejo de errores, CORS y arranque (`Program.cs`) |
| `Agro.Tests` | Pruebas unitarias de Application (sin base de datos) |

```
Agro/
├── Agro.slnx
├── src/
│   ├── Agro.Domain/Entities/                 13 entidades (scaffold de agro_db)
│   ├── Agro.Application/
│   │   ├── Common/Interfaces/IAgroDbContext.cs
│   │   ├── Salud/Queries/ComprobarSalud/     query + DTO de /api/salud
│   │   └── DependencyInjection.cs            AddApplication()
│   ├── Agro.Infrastructure/
│   │   ├── Configuration/                    lectura del .env y cadena de conexión
│   │   ├── Persistence/                      AgroDbContext + verificación de conexión
│   │   └── DependencyInjection.cs            AddInfraestructure()
│   └── Agro.Api/
│       ├── Controllers/SaludController.cs
│       ├── Middleware/ManejadorExcepciones.cs
│       ├── Program.cs
│       └── .env.example
└── tests/Agro.Tests/Application/Salud/        pruebas de ComprobarSalud
```

## Ejecutar en local

1. Enciende el contenedor de PostgreSQL y verifica que exista `agro_db` (scripts de la HU-03):
   ```bash
   docker start mi_postgres
   ```
2. Copia `Agro/src/Agro.Api/.env.example` como `Agro/src/Agro.Api/.env` y escribe tus datos:

   | Variable | Ejemplo | Descripción |
   |---|---|---|
   | `DB_HOST` | `localhost` | Servidor de PostgreSQL |
   | `DB_PORT` | `5432` | Puerto |
   | `DB_NAME` | `agro_db` | Base de datos |
   | `DB_USER` | `postgres` | Usuario |
   | `DB_PASSWORD` | — | Contraseña (solo en `.env`, nunca en Git) |
   | `FRONTEND_URL` | `http://localhost:5173` | Orígenes permitidos por CORS, separados por coma |

3. Ejecuta la API:
   - **Visual Studio:** abre `Agro/Agro.slnx` y presiona **F5** (se abre `/scalar` para probar la API).
   - **Terminal**, dentro de `Agro`:
     ```bash
     dotnet run --project src/Agro.Api
     ```
4. Abre `http://localhost:5222/api/salud` (o usa el archivo `Agro.Api.http` de Visual Studio).

## Pruebas unitarias

```bash
dotnet test Agro/Agro.slnx
```

## Servicio de salud · `GET /api/salud`

| Situación | Código | Respuesta |
|---|---|---|
| API y base de datos activas | 200 | `{"estado":"OK","baseDatos":"conectada","fecha":"…","mensaje":null}` |
| Base de datos sin conexión | 503 | `{"estado":"ERROR","baseDatos":"sin conexión","mensaje":"…"}` |

## Criterios de aceptación (HU-04)

| # | Criterio | Cómo se cumple |
|---|---|---|
| 1 | El servidor arranca sin errores | `dotnet run` / F5 |
| 2 | Se conecta a la base de datos y lo registra en la consola | `Conexión exitosa a la base de datos agro_db en localhost:5432` |
| 3 | `/api/salud` responde 200 y «OK» | `SaludController` → query `ComprobarSalud` |
| 4 | Si la conexión falla, mensaje claro sin exponer credenciales | Indica el motivo (contraseña incorrecta, base inexistente o contenedor apagado); la API sigue activa y `/api/salud` responde 503 |

Reglas de negocio:

- **Datos sensibles en `.env`:** la configuración se lee de variables de entorno. `.env` está en `.gitignore`. Si falta una variable, la API no arranca y dice cuál falta.
- **Respuestas en JSON:** los errores (400, 404, 500, 503) usan ProblemDetails (`application/problem+json`).
- **CORS:** solo se aceptan peticiones de los orígenes en `FRONTEND_URL`.

## Regenerar las entidades si cambia la base de datos

Dentro de `Agro`, con la cadena de conexión solo para la sesión de la terminal:

```powershell
$env:ConnectionStrings__Agro="Host=localhost;Port=5432;Database=agro_db;Username=TU_USUARIO;Password=TU_CONTRASEÑA"
```

```bash
dotnet ef dbcontext scaffold "Name=ConnectionStrings:Agro" Npgsql.EntityFrameworkCore.PostgreSQL --project src/Agro.Infrastructure --startup-project src/Agro.Api --output-dir ../Agro.Domain/Entities --namespace Agro.Domain.Entities --context-dir Persistence --context AgroDbContext --context-namespace Agro.Infrastructure.Persistence --no-onconfiguring --no-pluralize --force
```

`AgroDbContext.Interfaz.cs` es una clase `partial` aparte, así que no se pierde al regenerar.

> **Windows 11:** si aparece *"Could not load assembly … Una directiva de Control de aplicaciones bloqueó este archivo"*,
> el **Control inteligente de aplicaciones** está bloqueando las DLL compiladas; debe desactivarse en el equipo de desarrollo.

## Notas para las siguientes historias

- Fechas `TIMESTAMPTZ`: usar siempre `DateTime.UtcNow`.
- Cada historia agrega su carpeta en `Application/<Funcionalidad>/Commands|Queries/` y su controlador en `Api/Controllers`.
- Sesión con JWT y cierre por 20 minutos de inactividad: HU-10 y HU-11 (tabla `sesion`).
