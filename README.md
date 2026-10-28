# AgroPWA-Back

[![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=AlejandroGtz23_AgroPWA-Back&metric=alert_status)](https://sonarcloud.io/summary/overall?id=AlejandroGtz23_AgroPWA-Back)
[![Vulnerabilidades](https://sonarcloud.io/api/project_badges/measure?project=AlejandroGtz23_AgroPWA-Back&metric=vulnerabilities)](https://sonarcloud.io/summary/overall?id=AlejandroGtz23_AgroPWA-Back)
[![Code smells](https://sonarcloud.io/api/project_badges/measure?project=AlejandroGtz23_AgroPWA-Back&metric=code_smells)](https://sonarcloud.io/summary/overall?id=AlejandroGtz23_AgroPWA-Back)
[![Cobertura](https://sonarcloud.io/api/project_badges/measure?project=AlejandroGtz23_AgroPWA-Back&metric=coverage)](https://sonarcloud.io/summary/overall?id=AlejandroGtz23_AgroPWA-Back)

Proyecto especialmente dirigido para personas que trabajan directamente en el campo o cultivos.

API REST de **Agro**, la PWA para el registro y seguimiento de cultivos.
HU-04 · Configuración del entorno backend · HU-07 · Configuración del despliegue · Sprint 1
HU-29 · Análisis estático de calidad de código · HU-31 · Documentación de la API con Swagger · Sprint 3
HU-09 · Registro de usuario · HU-10 · Inicio de sesión · HU-11 · Cerrar sesión · Sprint 4 (backend)
HU-12 · Dashboard principal · HU-13 · Registrar parcela · Sprint 5 (backend)
Equipo: Victor Manuel Jose Jose · Jesus Alejandro Gutierrez Montufar (10 IDGS-G3, UTTT)

**API en producción (Azure):** `https://agropwa-api-cfcfcdbhdddqh7a7.centralus-01.azurewebsites.net`
· Salud: [`/api/salud`](https://agropwa-api-cfcfcdbhdddqh7a7.centralus-01.azurewebsites.net/api/salud)

**Base de datos:** las tablas y los procedimientos almacenados se versionan con Liquibase en
[vicmanujo/AgroPWA-BD](https://github.com/vicmanujo/AgroPWA-BD).

## Tecnologías

| Herramienta | Versión |
|---|---|
| .NET / ASP.NET Core Web API (controladores) | 10.0 LTS |
| Entity Framework Core + Npgsql (PostgreSQL) | 10.x |
| PostgreSQL (base `agro_db`) | 15 (Docker, desarrollo) · 16 (Azure) |
| FluentValidation | 12.x |
| BCrypt.Net-Next (contraseñas) | 4.x |
| JWT: `Microsoft.IdentityModel.JsonWebTokens` + `JwtBearer` | 8.x · 10.x |
| DotNetEnv (`.env`) · Swagger UI (documentación de la API) | 3.x · 10.x |
| xUnit + coverlet (pruebas y cobertura) | 2.x · 6.x |

## Arquitectura: Clean Architecture + CQRS ligero

```
Api ──► Application ──► Domain
 └──► Infrastructure ──┘
```

| Proyecto | Responsabilidad |
|---|---|
| `Agro.Domain` | Entidades de `agro_db` (Usuario, Cultivo, Sesion…). No depende de nada |
| `Agro.Application` | Casos de uso por funcionalidad: **Commands** (escriben) y **Queries** (leen), validadores, interfaces (`IAgroDbContext`, `I…Procedimientos`, `IGeneradorJwt`, `IHashContrasena`) y excepciones de negocio |
| `Agro.Infrastructure` | Implementaciones técnicas: `AgroDbContext`, llamadas a los **procedimientos almacenados**, BCrypt, JWT y lectura del `.env` |
| `Agro.Api` | Controladores, autenticación JWT, límite de peticiones, manejo de errores, CORS y arranque (`Program.cs`) |
| `Agro.Tests` | Pruebas unitarias de Application e Infrastructure/Security (sin base de datos) |

**CQRS ligero:** no se usa MediatR. Cada caso de uso es una clase `…Handler` con un método `EjecutarAsync`,
registrada en `AddApplication()` e inyectada en el controlador.

```
Agro/
├── Agro.slnx
├── src/
│   ├── Agro.Domain/Entities/                     14 entidades (scaffold de agro_db)
│   ├── Agro.Application/
│   │   ├── Auth/
│   │   │   ├── Commands/RegistrarUsuario/        command + handler + validador   (HU-09)
│   │   │   ├── Commands/IniciarSesion/           command + handler + validador   (HU-10)
│   │   │   ├── Commands/RenovarSesion/           command + handler + validador   (HU-10)
│   │   │   ├── Commands/CerrarSesion/            command + handler               (HU-11)
│   │   │   └── Common/                           CreadorSesion, ValidadorSesion, ReglasSesion, DTOs
│   │   ├── Parcelas/
│   │   │   ├── Commands/RegistrarParcela/        command + handler + validador   (HU-13)
│   │   │   ├── Commands/EditarParcela/           command + handler + validador   (HU-13)
│   │   │   ├── Queries/ObtenerMisParcelas/       handler                         (HU-13)
│   │   │   └── Common/                           ParcelaDto, ReglasParcela (reglas compartidas)
│   │   ├── Dashboard/
│   │   │   ├── Queries/ObtenerResumenDashboard/  handler                         (HU-12)
│   │   │   └── Common/                           ResumenDashboardDto, GuiaDashboardDto, ActividadRecienteDto
│   │   ├── Common/
│   │   │   ├── Exceptions/                       Conflicto (409), NoEncontrado (404), NoAutorizado (401), DemasiadosIntentos (429)
│   │   │   ├── Interfaces/                       IAgroDbContext, IAuthProcedimientos, IParcelasProcedimientos,
│   │   │   │                                     IDashboardProcedimientos, IGeneradorJwt, IHashContrasena
│   │   │   └── Models/                           datos que viajan a los procedimientos
│   │   ├── Salud/Queries/ComprobarSalud/         query + DTO de /api/salud
│   │   └── DependencyInjection.cs                AddApplication()
│   ├── Agro.Infrastructure/
│   │   ├── Configuration/                        lectura del .env, cadena de conexión y ConfiguracionJwt
│   │   ├── Persistence/                          AgroDbContext + verificación de conexión
│   │   │   └── Procedimientos/                   ProcedimientosBase (métodos comunes) + Auth, Parcelas y Dashboard
│   │   ├── Security/                             HashContrasenaBCrypt, GeneradorJwt
│   │   └── DependencyInjection.cs                AddInfraestructure()
│   └── Agro.Api/
│       ├── Configuration/                        AutenticacionJwt, DocumentacionApi (Swagger)
│       ├── Controllers/                          Salud, Auth, Parcelas y Dashboard
│       ├── Middleware/ManejadorExcepciones.cs
│       ├── Program.cs
│       └── .env.example
└── tests/Agro.Tests/
    ├── Application/Salud/                        pruebas de ComprobarSalud
    ├── Application/Auth/                         registro, login, sesión, validador + objetos falsos
    ├── Application/Parcelas/                     registrar, editar, consultar y reglas + falso en memoria
    ├── Application/Dashboard/                    resumen del dashboard + falso
    └── Infrastructure/Security/                  BCrypt y JWT
```

## Ejecutar en local

1. Enciende el contenedor de PostgreSQL y aplica las migraciones de
   [AgroPWA-BD](https://github.com/vicmanujo/AgroPWA-BD) (`liquibase update`):
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
   | `DB_SSLMODE` | `Require` | Solo para Azure (exige SSL). En local no se pone (se usa `Prefer`) |
   | `FRONTEND_URL` | `http://localhost:5173` | Orígenes permitidos por CORS, separados por coma |
   | `JWT_CLAVE` | — | Clave para firmar los tokens: **32 caracteres o más**, aleatoria y propia de cada entorno |
   | `JWT_EMISOR` | `agro-api` | Quién emite el token (`iss`) |
   | `JWT_AUDIENCIA` | `agro-pwa` | Para quién es el token (`aud`) |
   | `JWT_MINUTOS` | `20` | Duración del access token |

   > En el `.env` solo debe estar activa **una** configuración (local o Azure); la otra, comentada con `#`.

   Para generar una `JWT_CLAVE` en PowerShell:
   ```powershell
   [Convert]::ToBase64String((1..48 | ForEach-Object { Get-Random -Max 256 }))
   ```
3. Ejecuta la API:
   - **Visual Studio:** abre `Agro/Agro.slnx` y presiona **F5** (se abre `/swagger` para probar la API).
   - **Terminal**, dentro de `Agro`:
     ```bash
     dotnet run --project src/Agro.Api
     ```
4. Abre `http://localhost:5222/api/salud` o `http://localhost:5222/swagger`.

## Autenticación (HU-09, HU-10, HU-11)

### Endpoints

| Método y ruta | Protegido | Respuestas |
|---|---|---|
| `POST /api/auth/registro` | No | **201** sesión · **400** errores por campo · **409** el correo ya existe |
| `POST /api/auth/login` | No (máx. 10 por minuto por IP) | **200** sesión · **400** · **401** «Correo o contraseña incorrectos.» · **429** cuenta bloqueada o demasiadas peticiones |
| `POST /api/auth/renovar` | No (usa el refresh token) | **200** tokens nuevos · **400** · **401** sesión vencida o cerrada |
| `GET /api/auth/yo` | `[Authorize]` | **200** `{ id, correo, rol }` · **401** |
| `POST /api/auth/logout` | `[Authorize]` | **204** sesión cerrada · **401** ya estaba cerrada |

La sesión que devuelven registro, login y renovar:

```json
{
  "accessToken": "eyJ…",
  "refreshToken": "…",
  "expiraUtc": "2026-10-22T21:00:00Z",
  "usuario": { "id": 1, "nombre": "Juan", "correo": "juan@correo.com", "rol": "productor" }
}
```

Las rutas protegidas reciben el encabezado `Authorization: Bearer <accessToken>`.

### Cómo funciona la sesión

| Regla | Implementación |
|---|---|
| Contraseñas | BCrypt (costo 12). Se comparan en C#: la contraseña nunca viaja a la base en texto plano |
| Access token | JWT HS256 de `JWT_MINUTOS` minutos con `sub`, `email`, `role` y `jti` (identifica la sesión) |
| Refresh token | 64 bytes aleatorios. En la tabla `sesion` solo se guarda su **hash SHA-256** |
| Duración de la sesión | 7 días con «Recordarme» · 12 horas sin él |
| Renovar | Rota el `jti` y el refresh token: el refresh anterior deja de servir |
| Validación en cada petición | Después de validar la firma, `sp_validar_sesion` comprueba que la sesión siga abierta: la cierra si venció o si pasó **20 minutos sin actividad** (no aplica con «Recordarme») |
| Bloqueo | Al **5.º intento fallido** la cuenta se bloquea **15 minutos** (`intento_acceso`, `bloqueado_hasta`) |
| Mensaje de error | Si el correo no existe o la contraseña está mal, el mensaje es el mismo |
| Límite de peticiones | 10 por minuto por IP en `/login` (fuerza bruta) |
| Cerrar sesión | `sp_cerrar_sesion` marca `fecha_cierre` y `motivo_cierre = 'logout'`; el access token y el refresh token dejan de servir aunque no hayan vencido |

## Parcelas (HU-13)

Todas las rutas llevan `[Authorize]`; el usuario sale del token, así que nadie puede ver ni editar parcelas ajenas.

| Método y ruta | Respuestas |
|---|---|
| `GET /api/parcelas` | **200** `[ { id, idLocal, nombre, superficieHa, ubicacionReferencia, numeroCultivos } ]`, ordenadas por nombre |
| `POST /api/parcelas` | **201** parcela nueva · **200** si el `idLocal` ya existía (devuelve la misma, sin duplicar) · **400** · **409** |
| `PUT /api/parcelas/{id}` | **200** parcela editada · **400** · **404** no existe o no es tuya · **409** |

```json
{ "idLocal": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "nombre": "Parcela Norte", "superficieHa": 1.5, "ubicacionReferencia": "San Marcos" }
```

| Regla | Dónde se cumple |
|---|---|
| Nombre obligatorio (máx. 80) y **único por usuario** sin distinguir mayúsculas → 409 «Ya tienes una parcela con ese nombre.» | `ReglasParcela` + `sp_registrar_parcela` / `sp_editar_parcela` |
| Superficie > 0, máximo 999,999.99 ha y **2 decimales** («Ingresa un número válido.») | `ReglasParcela` (columna `NUMERIC(8,2)`) |
| Ubicación opcional (máx. 150); vacía se guarda como `null` | `ReglasParcela.Normalizar` |
| `idLocal` (UUID generado en el dispositivo): si llega repetido, se devuelve la parcela existente | `sp_registrar_parcela` → `p_ya_existia` |
| Editar reemplaza **todos** los campos: el frontend envía la parcela completa | `sp_editar_parcela` |

Registrar y editar comparten las reglas con métodos de extensión de FluentValidation (`NombreParcela()`,
`Superficie()`, `Ubicacion()`), así no hay código duplicado.

## Dashboard (HU-12)

`GET /api/dashboard/resumen` (`[Authorize]`) → **200** · **404** si el usuario no existe o está desactivado.

```json
{ "nombre": "Juan", "cultivosActivos": 4, "avancesDelMes": 7,
  "guia": { "tieneParcela": true, "tieneCultivo": true, "tieneAvance": false },
  "actividadReciente": [ { "tipo": "Riego", "cultivo": "Maíz criollo", "fecha": "2026-10-08T14:30:00Z" } ] }
```

- Todos los totales son **solo del usuario del token**.
- **Avances del mes** se cuentan con el mes en hora de México (`America/Mexico_City`), no en UTC.
- `actividadReciente`: los últimos 5 avances, del más reciente al más antiguo. Las fechas van en UTC.
- Un usuario sin datos recibe todo en `0`, la guía en `false` y la actividad vacía (criterio 3).
- **Pendientes de sincronizar** no viene de la API: lo cuenta el frontend en IndexedDB.

## Acceso a datos: solo procedimientos almacenados

Por seguridad, la API **no hace consultas directas** a las tablas. Las clases de `Persistence/Procedimientos`
ejecutan procedimientos almacenados con parámetros tipados (`NpgsqlParameter`), y el usuario `agro_app`
**solo tiene permiso `EXECUTE`** (migración 010 de AgroPWA-BD).

| Procedimiento | Uso | Migración |
|---|---|---|
| `sp_registrar_usuario` | Crea el usuario (rol productor); error 23505 → 409 | 006 |
| `sp_obtener_usuario_login` | Datos para el login (cursor) | 007 |
| `sp_registrar_intento_acceso` | Guarda el intento y aplica el bloqueo | 007 |
| `sp_crear_sesion` | Crea la sesión con el `jti` y el hash del refresh token | 007 |
| `sp_validar_sesion` | Valida y actualiza la actividad de la sesión | 008 |
| `sp_renovar_sesion` | Rota `jti` y refresh token (cursor con el usuario) | 008 |
| `sp_cerrar_sesion` | Cierra la sesión (logout) | 009 |
| `sp_registrar_parcela` | Registra la parcela o devuelve la existente por `idLocal`; nombre repetido → 409 | 011 |
| `sp_editar_parcela` | Edita una parcela del usuario; `false` → 404 | 011 |
| `sp_obtener_parcelas` | Parcelas activas con su número de cultivos (cursor) | 011 |
| `sp_obtener_resumen_dashboard` | Totales y pasos de la guía (cursor) | 012 |
| `sp_obtener_actividad_reciente` | Últimos avances: tipo, cultivo y fecha (cursor) | 012 |

`ProcedimientosBase` tiene los métodos comunes que heredan `AuthProcedimientos`, `ParcelasProcedimientos` y
`DashboardProcedimientos`:

| Método | Para |
|---|---|
| `EjecutarAsync` | `CALL` con un solo valor `INOUT` o ninguno (`ExecuteScalar`; devuelve `null` si no hay fila) |
| `EjecutarFilaAsync` | `CALL` con varios valores `INOUT` (lee la única fila, p. ej. `p_id_parcela` y `p_ya_existia`) |
| `ConsultarAsync` | Procedimientos de lectura: en una transacción, `CALL …` y luego `FETCH ALL FROM resultado` |
| `Parametro` | Crea un `NpgsqlParameter` tipado (`null` → `DBNull.Value`) |

Los procedimientos son `SECURITY DEFINER` con `search_path = public`.

## Respuestas de error

Todos los errores son JSON (ProblemDetails) en español:

| Código | Cuándo | Origen |
|---|---|---|
| 400 | Datos que no cumplen las reglas · JSON con un tipo incorrecto (letras en un número, fecha inválida) | FluentValidation · `InvalidModelStateResponseFactory` en `Program.cs` |
| 401 | Sin token, token vencido o sesión cerrada · credenciales incorrectas | JwtBearer · `NoAutorizadoException` |
| 404 | El recurso no existe o es de otro usuario | `NoEncontradoException` |
| 409 | Dato repetido (correo, nombre de parcela) | `ConflictoException` |
| 429 | Cuenta bloqueada o demasiadas peticiones al login | `DemasiadosIntentosException` · rate limiter |
| 503 | Sin conexión con la base de datos | `ManejadorExcepciones` |

Los 400 incluyen los errores por campo: `{ "errores": { "superficieHa": ["Ingresa un número válido."] } }`.

## Pruebas unitarias

```bash
dotnet test Agro/Agro.slnx
```

Con cobertura (genera `coverage.cobertura.xml` en `TestResults/`):

```bash
dotnet test Agro/Agro.slnx --collect:"XPlat Code Coverage"
```

| Archivo | Qué prueba |
|---|---|
| `Application/Salud/ComprobarSaludHandlerTests` | `/api/salud` con base conectada, sin conexión y con error |
| `Application/Auth/IniciarSesionHandlerTests` | Login correcto, contraseña incorrecta, correo inexistente, cuenta bloqueada, bloqueo vencido, datos vacíos |
| `Application/Auth/RegistrarUsuarioHandlerTests` | Registro con rol productor, contraseña con hash, correo en minúsculas, opcionales en null, correo duplicado (409) |
| `Application/Auth/RegistrarUsuarioValidatorTests` | Cada regla del formulario de registro, incluidos los campos que no llegan (`null`) |
| `Application/Auth/SesionTests` | Duración con y sin «Recordarme», validar, renovar y cerrar sesión |
| `Application/Parcelas/ParcelasHandlersTests` | Registrar (201/200 por `idLocal`, espacios, 409, 400), editar (200, 404 propio y ajeno, 409, 400) y consultar solo las propias |
| `Application/Parcelas/ReglasParcelaTests` | Nombre, superficie (0, negativa, 3 decimales, máximo), ubicación, un solo mensaje por campo y `Normalizar` |
| `Application/Dashboard/ObtenerResumenDashboardHandlerTests` | Usuario sin datos en 0, totales y guía, actividad con máximo 5, usuario inexistente (404) |
| `Infrastructure/Security/SeguridadTests` | BCrypt, contenido del JWT y refresh token (aleatorio + SHA-256) |

**80 pruebas.** Ninguna usa base de datos: los archivos de falsos (`Falsos.cs`, `ParcelasFalsas.cs`,
`DashboardProcedimientosFalsos.cs`) reemplazan los procedimientos almacenados, el JWT y el hash por objetos que
devuelven lo que cada prueba necesita y guardan lo que reciben. `ParcelasFalsas.cs` aplica en memoria las mismas
reglas que los procedimientos de la migración 011.

## Calidad de código · SonarQube Cloud (HU-29)

Cada push y Pull Request se analiza automáticamente con **SonarQube Cloud** (análisis estático):
bugs, vulnerabilidades, *code smells*, duplicación, complejidad y cobertura de pruebas.
El **Quality Gate** decide si el código cumple el estándar y su resultado aparece en el PR.

- Panel del proyecto: [sonarcloud.io · AgroPWA-Back](https://sonarcloud.io/summary/overall?id=AlejandroGtz23_AgroPWA-Back)
- Workflow: [`.github/workflows/calidad-codigo.yml`](.github/workflows/calidad-codigo.yml)

### Configuración

| Elemento | Valor |
|---|---|
| Organización (SonarQube Cloud) | `alejandrogtz23` · plan Free (repositorio público) |
| Proyecto | `AlejandroGtz23_AgroPWA-Back` |
| Método de análisis | GitHub Actions + SonarScanner for .NET (*Automatic Analysis* desactivado: no es compatible con C#) |
| Rama principal en Sonar | `develop` (el plan Free solo muestra el detalle completo de la rama principal) |
| Cobertura | `coverlet.collector` en formato OpenCover (`coverage.opencover.xml`) |
| Quality Gate | Código nuevo: calificación A en confiabilidad, seguridad y mantenibilidad · **cobertura ≥ 80 %** · duplicación ≤ 3 % · hotspots revisados |
| Exclusiones de cobertura | `Agro.Api/**`, `Infrastructure/Persistence/**`, `Infrastructure/Configuration/**`, `DependencyInjection.cs` y `Domain/Entities/**` |

Las exclusiones solo quitan esos archivos del **cálculo de cobertura**: necesitan la base real o el servidor web
para ejecutarse (se prueban con Swagger y en Azure). Sonar sigue revisándolos en busca de bugs y vulnerabilidades.

Secreto del repositorio (**Settings → Secrets and variables → Actions**):

| Secreto | Contenido |
|---|---|
| `SONAR_TOKEN` | Token de análisis generado en SonarQube Cloud (nunca en el código) |

### Ejecución

| Evento | Qué hace |
|---|---|
| Push a `main`, `develop` o `Sprint*` | Compila, ejecuta las pruebas con cobertura y envía el análisis |
| Pull Request hacia `Sprint*`, `develop`, `release` o `main` | Igual; el PR muestra el check del análisis |
| Manual | GitHub → Actions → *Calidad · Análisis SonarQube Cloud* → *Run workflow* |

El scanner trabaja en tres fases: `begin` (se engancha al compilador) → `dotnet build` y `dotnet test` → `end` (envía los resultados).

### Resultados

| Métrica | Sprint 3 (inicial) | Sprint 3 (final) | Sprint 4 | Sprint 5 |
|---|---|---|---|---|
| Vulnerabilidades | 1 (Seguridad **C**) | 0 (Seguridad **A**) | 0 | 0 |
| Code smells | 3 | 0 | 0 | 0 |
| Bugs | 0 | 0 | 0 | 0 |
| Cobertura del código nuevo | — | — | **100 %** | **98 %** |
| Quality Gate | — | **Passed** | **Passed** | **Passed** |

| Regla | Archivo | Corrección |
|---|---|---|
| S7637 | `despliegue-api.yml` | `azure/webapps-deploy` fijado por SHA completo (v3.0.8) |
| S6966 | `Program.cs` | `await Console.Error.WriteLineAsync(...)` |
| S6966 | `Program.cs` | `await app.RunAsync()` |
| CA1873 | `Program.cs` | `IsEnabled(LogLevel.Information)` antes del log |

Detalle de las correcciones: PR [#24](https://github.com/AlejandroGtz23/AgroPWA-Back/pull/24).
En el Sprint 4 el Quality Gate falló al inicio por **0 % de cobertura** del código nuevo; se resolvió con
las pruebas de autenticación y las exclusiones de cobertura (PR [#30](https://github.com/AlejandroGtz23/AgroPWA-Back/pull/30)).
En el Sprint 5 pasó lo mismo con parcelas y el dashboard: cada funcionalidad nueva se sube junto con sus pruebas.

## Documentación de la API · Swagger (HU-31)

La documentación tiene dos partes:

| Ruta | Qué es | Quién la genera |
|---|---|---|
| `/openapi/v1.json` | Documento OpenAPI: describe todos los endpoints, parámetros y respuestas | .NET 10 (`AddOpenApi()` + `MapOpenApi()`) |
| `/swagger` | **Swagger UI**: página que lee ese documento y permite probar cada endpoint con *Try it out* | `Swashbuckle.AspNetCore.SwaggerUI` (`UseSwaggerUI()`) |

- Solo se publica en **desarrollo** (`ASPNETCORE_ENVIRONMENT=Development`); en Azure (producción) ambas rutas responden 404.
- Para probarla: ejecuta la API, abre `http://localhost:5222/swagger`, despliega `GET /api/salud` → *Try it out* → *Execute*.
- **Rutas protegidas:** ejecuta `POST /api/auth/login`, copia el `accessToken`, da clic en **Authorize** y pégalo
  (sin la palabra `Bearer`). Después `GET /api/auth/yo` responde 200.

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
- **Respuestas en JSON:** los errores (400, 401, 404, 409, 429, 500, 503) usan ProblemDetails (`application/problem+json`).
- **CORS:** solo se aceptan peticiones de los orígenes en `FRONTEND_URL`.

## Despliegue en Azure (HU-07)

### Arquitectura

```
                       GitHub · AlejandroGtz23/AgroPWA-Back
   Pull Request ─► ✅ Compilar y probar        merge a main ─► ✅ Compilar y probar ─► 🚀 Desplegar
                                                                       │
Azure · grupo de recursos rg-agropwa-dev                               ▼
┌──────────────────────────┐  HTTPS  ┌────────────────────────────────┐   SSL   ┌───────────────────────────┐
│ agropwa-front            │ ──────► │ agropwa-api                    │ ──────► │ agropwa-db-2              │
│ Static Web App (React)   │         │ App Service · .NET 10 · Linux  │         │ PostgreSQL 16 Flexible    │
│                          │         │ plan F1 (gratis) · Central US  │         │ B1ms · 32 GB · Central US │
└──────────────────────────┘         └────────────────────────────────┘         └───────────────────────────┘
```

### Recursos

| Recurso | Servicio | Región | Plan |
|---|---|---|---|
| `agropwa-api` | Azure App Service (Linux, .NET 10 LTS) | Central US | F1 Gratis |
| `plan-agropwa` | Plan de App Service | Central US | F1 Gratis |
| `agropwa-db-2` | Azure Database for PostgreSQL Flexible Server 16 | Central US | Burstable B1ms, 32 GB |
| `agropwa-front` | Azure Static Web Apps (frontend, HU-05) | Global | — |

La API y la base de datos están en la **misma región** para reducir la latencia y evitar costos de
transferencia entre regiones.

### Base de datos en Azure

- Las tablas y procedimientos se aplican con las migraciones de **AgroPWA-BD** (Liquibase).
- Conexión obligatoriamente cifrada (`DB_SSLMODE=Require`).
- Firewall: solo IP autorizadas y servicios de Azure.
- **Mínimo privilegio:** la API usa el usuario `agro_app`, que **solo puede ejecutar procedimientos almacenados**
  (sin `SELECT`, `INSERT`, `UPDATE` ni `DELETE` directo en las tablas). El administrador (`agro_admin`) solo se
  usa para migraciones y mantenimiento.

### Configuración de la API en Azure

En Azure no hay archivo `.env`: la configuración está en **agropwa-api → Configuración → Variables de entorno**.

| Variable | Valor |
|---|---|
| `DB_HOST` | `agropwa-db-2.postgres.database.azure.com` |
| `DB_PORT` | `5432` |
| `DB_NAME` | `agro_db` |
| `DB_USER` | `agro_app` |
| `DB_PASSWORD` | (contraseña de `agro_app`, solo en Azure) |
| `DB_SSLMODE` | `Require` |
| `FRONTEND_URL` | URL de la Static Web App y `http://localhost:5173` (separados por coma) |
| `JWT_CLAVE` | Clave propia de producción (≥ 32 caracteres, distinta a la de desarrollo) |
| `JWT_EMISOR` | `agro-api` |
| `JWT_AUDIENCIA` | `agro-pwa` |
| `JWT_MINUTOS` | `20` |
| `ASPNETCORE_FORWARDEDHEADERS_ENABLED` | `true` (el límite de peticiones usa la IP real del cliente detrás del proxy de Azure) |

### Integración y despliegue continuo (GitHub Actions)

Workflow: [`.github/workflows/despliegue-api.yml`](.github/workflows/despliegue-api.yml)

| Evento | Qué hace |
|---|---|
| Pull Request hacia `Sprint*`, `develop`, `release` o `main` | Restaura, compila en Release y ejecuta las pruebas. **No despliega**. El PR muestra ✅ o ❌ |
| Push / merge a `main` | Compila, prueba, publica y **despliega** en `agropwa-api` |
| Manual | GitHub → Actions → *Run workflow* |

Secreto del repositorio (**Settings → Secrets and variables → Actions**):

| Secreto | Contenido |
|---|---|
| `AZURE_WEBAPP_PUBLISH_PROFILE` | Perfil de publicación de `agropwa-api` (requiere la autenticación básica de SCM habilitada) |

### Flujo de ramas

```
SprintN_Victor ─┐
                ├─► SprintN ─► develop ─► release ─► main ─► 🚀 Azure
SprintN_Ale    ─┘
```

Cada sprint crea sus ramas desde `develop` (`SprintN`, `SprintN_Victor`, `SprintN_Ale`).
Todo cambio llega a `main` por Pull Request; cada PR ejecuta la compilación, las pruebas y el análisis de calidad.

### Verificación del despliegue (Sprint 4)

| Prueba | Resultado |
|---|---|
| `GET /api/salud` en Azure | 200 · `{"estado":"OK","baseDatos":"conectada"}` |
| `GET /api/auth/yo` sin token | 401 |
| `POST /api/auth/login` con un correo inexistente | 401 «Correo o contraseña incorrectos.» (procedimientos ejecutados con `agro_app`) |
| `POST /api/auth/renovar` con un refresh token falso | 401 «Tu sesión expiró. Inicia sesión de nuevo.» |
| Ruta inexistente | 404 en JSON (ProblemDetails) |
| Petición CORS desde un origen no permitido | Bloqueada |
| Documentación `/swagger` y `/openapi/v1.json` | Solo en desarrollo (404 en producción) |

### Verificación del despliegue (Sprint 5)

Con una cuenta de prueba en producción, después de aplicar las migraciones 011 y 012 en Azure:

| Prueba | Resultado |
|---|---|
| `GET /api/parcelas` y `GET /api/dashboard/resumen` sin token | 401 |
| Dashboard de un usuario nuevo | 200 · todo en 0, guía en `false` |
| `POST /api/parcelas` · otra vez con el mismo `idLocal` | 201 · 200 con el mismo `id` |
| Nombre repetido · datos inválidos · letras en la superficie | 409 · 400 · 400 en español |
| `PUT /api/parcelas/{id}` · id inexistente | 200 · 404 |
| Dashboard después de registrar la parcela | `tieneParcela: true` |
| Logout y luego usar el mismo token | 204 · 401 |

> **Plan gratuito F1:** la app se suspende tras unos 20 minutos sin uso; la primera petición después
> puede tardar de 10 a 30 segundos. Para no consumir crédito, el servidor PostgreSQL puede detenerse
> desde el portal cuando no se use.

## Regenerar las entidades si cambia la base de datos

Dentro de `Agro`, con la cadena de conexión solo para la sesión de la terminal:

```powershell
$env:ConnectionStrings__Agro="Host=localhost;Port=5432;Database=agro_db;Username=TU_USUARIO;Password=TU_CONTRASEÑA"
```

```bash
dotnet ef dbcontext scaffold "Name=ConnectionStrings:Agro" Npgsql.EntityFrameworkCore.PostgreSQL --project src/Agro.Infrastructure --startup-project src/Agro.Api --output-dir ../Agro.Domain/Entities --namespace Agro.Domain.Entities --context-dir Persistence --context AgroDbContext --context-namespace Agro.Infrastructure.Persistence --no-onconfiguring --no-pluralize --force --table rol --table usuario --table dispositivo --table sesion --table codigo_verificacion --table intento_acceso --table tipo_cultivo --table tipo_actividad --table parcela --table cultivo --table avance --table fotografia --table cola_sincronizacion --table mensaje_contacto
```

- La lista `--table` es necesaria para que **no** se generen entidades de las tablas internas de Liquibase
  (`databasechangelog`, `databasechangeloglock`). Si una migración agrega una tabla, agrégala a la lista.
- `AgroDbContext.Interfaz.cs` es una clase `partial` aparte, así que no se pierde al regenerar.
- Después de regenerar, si `IAgroDbContext` tiene un `DbSet` nuevo, agrégalo también al contexto falso de las pruebas.

> **Windows 11:** si aparece *"Could not load assembly … Una directiva de Control de aplicaciones bloqueó este archivo"*,
> el **Control inteligente de aplicaciones** está bloqueando las DLL compiladas; debe desactivarse en el equipo de desarrollo.

## Notas para las siguientes historias

- Fechas `TIMESTAMPTZ`: usar siempre `DateTime.UtcNow` (o `TimeProvider` para poder probarlo).
- Cada historia agrega su carpeta en `Application/<Funcionalidad>/Commands|Queries/` y su controlador en `Api/Controllers`.
- **Acceso a datos:** cada operación nueva es un procedimiento almacenado en AgroPWA-BD (migración nueva), una
  interfaz en `Application/Common/Interfaces` y su implementación en `Infrastructure/Persistence/Procedimientos`.
- Las rutas privadas llevan `[Authorize]`; el id del usuario sale del claim `sub` del token, nunca del cuerpo de la petición.
- Cada handler nuevo lleva sus pruebas en `tests/Agro.Tests` para mantener la cobertura del código nuevo ≥ 80 %.
- Las clases de procedimientos nuevas heredan de `ProcedimientosBase`; un procedimiento con varios `INOUT` se lee
  con `EjecutarFilaAsync`.
- Si dos commands validan los mismos campos, las reglas van en un `Reglas…` compartido (como `ReglasParcela`).
- **Orden del despliegue:** primero las migraciones en Azure (AgroPWA-BD) y después el merge del backend a `main`;
  si no, los endpoints nuevos responden 500 porque no encuentran sus procedimientos.
