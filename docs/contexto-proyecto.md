# Itzyolotl — Contexto técnico para continuidad entre chats

Última actualización: 23/11/2026 (chat 1). Quien avance el proyecto debe actualizar §7 al cerrar cada sección.

Leyenda: ✅ confirmado por el usuario con salida o resultado visible · ⚠️ indicado en el chat 1 pero sin confirmación explícita · ❌ no existe todavía.

## 0. Instrucciones para el nuevo chat

- No asumas que conoces el proyecto ni inventes configuraciones, archivos o implementaciones.
- No des por hecha una funcionalidad sin evidencia (archivo o salida). Antes de actuar, revisa este contexto y pide los archivos, configuraciones o salidas necesarios para verificar el estado real (lista en §7.3).
- No repitas tareas completadas (§7.1).
- Respeta la arquitectura y las decisiones de §4 y §9. Si propones cambiarlas, explica antes el motivo y espera aprobación.
- Explica los cambios paso a paso: qué archivo se modifica, qué hace y por qué. No hagas cambios grandes sin explicar antes su propósito.
- No modifiques código que el usuario no te haya mostrado.
- El chat 1 queda detenido; el nuevo chat continúa. Un solo chat activo a la vez. Coordinación en §11.
- No fusiones ramas ni modifiques `main` (o la rama estable) sin autorización expresa del usuario.
- Estilo de trabajo en §2.

## 1. Proyecto

Aplicación web para **Itzyolotl** ("Crepas y paninis · Corazón de obsidiana"), cafetería nocturna pequeña: 4 mesas (dos de 2 y dos de 3 personas). Vende cafés (capuchino y otros), crepas dulces y saladas, frappes y paninis. No tiene sitio web.

- Objetivo: demo para ofrecérsela al dueño; si no la acepta, sirve de portafolio.
- Problema: el cliente debe ir al local, escribir su pedido a mano (nombre, comer aquí/llevar), a veces lo olvida, y vuelve a preguntar si ya está.
- Horario real aproximado: abre ~19:00 y deja de atender ~23:00; no cruza medianoche. Días de apertura sin confirmar.
- El menú, precios y horario sembrados en la base son **inventados** (solo desarrollo).
- Marca (medida de un anuncio del local): negro `#000000`, azul `#02b7fe`, blanco. Títulos en mayúsculas muy gruesas. Elegidos por el chat 1: superficie `#0d0f10`, borde `#1c2a31`, texto suave `#a9b4ba`. Sobre fondo azul el texto va **negro** (contraste ~9:1; blanco ~2,3:1). Confirmar el azul con el archivo original del logo.
- Los clientes usan el celular de noche: diseño oscuro y mobile-first.

## 2. Estilo de trabajo acordado

- El usuario aprende mientras construye: una sección a la vez, fragmentos pequeños, indicar **siempre el archivo**, explicar qué hace y por qué.
- Etiquetar conceptos: 🟢 conocido · 🟡 para estudiar · 🔴 avanzado. No evitar técnicas avanzadas; explicar para qué sirven y qué estudiar.
- Domina C#, .NET y SQL Server. **No asumir** dominio de HTML, CSS, Tailwind ni JavaScript.
- Responder en español. Pedir con comandos exactos las salidas o archivos a verificar, en lugar de adivinar. Verificar con `dotnet build` y `dotnet build -c Release`; el usuario hace commit al cerrar cada sección.
- Si algo falla, el usuario pega el error; no improvisar cambios.

## 3. Entorno y tecnologías

| Área | Detalle |
|---|---|
| Sistema | Windows 11 (build 26200), Visual Studio 2026 (18.10.3), terminal PowerShell y Git Bash |
| .NET | SDK 10.0.302 y 10.0.401 (solo 10.x; no hay SDK 11), runtime 10.0.12 |
| Backend | ASP.NET Core Web API con minimal APIs, EF Core 10.0.15 |
| Frontend | Blazor WebAssembly standalone (`Microsoft.NET.Sdk.BlazorWebAssembly`) |
| Estilos | Tailwind CSS 4.3.5, CLI independiente en `tools/tailwindcss.exe` (**sin Node ni npm**) |
| Orquestación | .NET Aspire 17.0.0 (AppHost con `Microsoft.NET.Sdk` y paquetes, no el SDK `Aspire.AppHost.Sdk`) |
| Base de datos | SQL Server 2022 en contenedor Docker (imagen `mcr.microsoft.com/mssql/server`), SSMS 2022 para consultar |
| Contenedores | Docker Desktop 4.99.0 (Engine 29.9.3) sobre WSL2; WSL 3.0.5 |
| Pruebas | xUnit 2.9.3 |
| Telemetría | OpenTelemetry 1.18.0 vía ServiceDefaults; dashboard de Aspire |
| Migraciones | `dotnet-ef` 10.0.15 como herramienta local (`dotnet-tools.json` en la raíz) |

El usuario también tiene SQL Server 2022 Developer instalado en local; **el proyecto no lo usa**, usa el contenedor.

## 4. Arquitectura

Monolito modular con capas (estilo Clean Architecture pragmático). Regla: las dependencias solo apuntan hacia adentro.

```
Domain  <-  Application  <-  Infrastructure
                 ^                 ^
Contracts  <-----+-----  Api  ----+      (Api compone todo)
   ^
   +---- Web.Customer (solo conoce Contracts)
AppHost orquesta: sql -> itzyolotl (db) -> api -> customer
```

| Proyecto | Responsabilidad | Referencias |
|---|---|---|
| `Domain` | Entidades y reglas puras. Sin EF ni SQL | ninguna |
| `Application` | Casos de uso (handlers), `IAppDbContext`, registro de servicios. Usa `Microsoft.EntityFrameworkCore` (sin proveedor) para escribir consultas | Domain, Contracts |
| `Infrastructure` | EF Core, `AppDbContext`, configuraciones, migraciones, seeder de desarrollo, integración Aspire con SQL Server | Application |
| `Contracts` | DTOs compartidos entre API y frontends | ninguna |
| `Api` | Endpoints, CORS, composición | Application, Infrastructure, Contracts, ServiceDefaults |
| `ServiceDefaults` | Telemetría, health checks, resiliencia HTTP, descubrimiento de servicios | — |
| `AppHost` | Orquestación local con Aspire | Api |
| `Web.Customer` | Frontend público (Blazor WASM) | Contracts |
| `tests/Domain.Tests` | Pruebas xUnit de Domain | Domain |

Frontends previstos (❌ aún no creados): `Web.Staff` (trabajadores y administrador, con login) y una librería `UI` de componentes compartidos. Motivo de separar: el celular del cliente no debe descargar el código del panel de gestión.

## 5. Estructura y archivos importantes

Raíz `C:\Dev\Itzyolotl`: `Itzyolotl.slnx`, `Directory.Build.props`, `Directory.Packages.props`, `dotnet-tools.json`, `.gitignore`, `.editorconfig`, `tools/`, `src/`, `tests/`, `docs/`, `scripts/`.

**Domain**
- `Common/Entity.cs`: `int Id { get; protected set; }`.
- `Entities/Category.cs`: `Name`, `DisplayOrder`; `Rename`.
- `Entities/Product.cs`: `CategoryId`, `Name`, `Description?`, `Price` (decimal), `PreparationMinutes`, `IsAvailable`; `ChangePrice`, `SetAvailability`. Setters privados, validación en constructor.
- `Entities/OpeningPeriod.cs`: `Day` (DayOfWeek), `OpensAt`, `ClosesAt` (TimeOnly). Exige cierre > apertura (**no soporta cierre pasada la medianoche**).
- `Schedule/OpeningSchedule.cs`: función pura `Evaluate(periods, localNow)` → `StoreStatus(IsOpen, NextChangeLocal)`. Abierto si `abre <= ahora < cierra`.

**Application**
- `Abstractions/IAppDbContext.cs`: `DbSet` de `Categories`, `Products`, `OpeningPeriods`.
- `Menu/GetMenuHandler.cs`: una consulta con `join`, `AsNoTracking`, agrupa por categoría en memoria.
- `Store/GetStoreStatusHandler.cs`: usa `TimeProvider` y la zona fija `America/Mexico_City`; devuelve estado, próximo cambio (`DateTimeOffset` con desfase) y horario semanal de los 7 días.
- `DependencyInjection.cs`: `AddApplication()` (`TryAddSingleton(TimeProvider.System)`, handlers `Scoped`).

**Contracts**
- `Menu/MenuDto.cs`: `MenuDto`, `MenuCategoryDto`, `MenuProductDto`.
- `Store/StoreStatusDto.cs`: `StoreStatusDto(IsOpen, NextChangeAt, TimeZoneId, WeeklySchedule)`, `DayScheduleDto`, `OpeningPeriodDto`.

**Infrastructure**
- `Persistence/AppDbContext.cs` (implementa `IAppDbContext`, `ApplyConfigurationsFromAssembly`).
- `Persistence/Configurations/` (`Category`, `Product`, `OpeningPeriod`).
- `Persistence/DesignTimeDbContextFactory.cs`: cadena falsa solo para que `dotnet ef` lea el modelo.
- `Persistence/Migrations/`: `InitialCreate`, `AddOpeningPeriods`.
- `DependencyInjection.cs`: `AddInfrastructure()` (`AddSqlServerDbContext<AppDbContext>("itzyolotl")` y `IAppDbContext` scoped) y `ApplyMigrationsAsync(IHost)`.
- `DevelopmentDataSeeder.cs`: `SeedDevelopmentDataAsync`, idempotente por tabla. Siembra 5 categorías (Cafés, Crepas saladas, Crepas dulces, Frappes, Paninis), 10 productos de ejemplo y tramos lunes–sábado.

**Api** — `Program.cs`: `AddServiceDefaults()` (**una sola vez**), `AddInfrastructure()`, `AddApplication()`, `AddOpenApi()`, política CORS `"Web"` leída de `Cors:AllowedOrigins`, `MapDefaultEndpoints()`, `UseCors("Web")`. En Desarrollo: `MapOpenApi`, `ApplyMigrationsAsync`, `SeedDevelopmentDataAsync`. Endpoints: `GET /api/menu` y `GET /api/store/status`.

**ServiceDefaults** — `Extensions.cs`: `AddServiceDefaults`, `ConfigureOpenTelemetry`, `AddDefaultHealthChecks`, `MapDefaultEndpoints` (`/health`, `/alive` en Desarrollo).

**AppHost** — `AppHost.cs` (no existe `Program.cs`).

**Web.Customer**
- `Program.cs`: lee `ApiBaseUrl` de `wwwroot/appsettings.json`; registra `HttpClient`, `MenuApiClient`, `StoreApiClient` (scoped).
- `Services/MenuApiClient.cs`, `Services/StoreApiClient.cs`: únicos que conocen las rutas de la API.
- `Formatting/SpanishText.cs`: hora y "Abre/Cierra…" en español sin depender de la cultura del navegador.
- `Components/StoreStatusBadge.razor(.cs)`: punto verde/rojo, "Abierto/Cerrado", próxima hora; se actualiza cada 60 s con `PeriodicTimer`; conserva el último estado si falla una actualización; `IDisposable`.
- `Layout/MainLayout.razor`: encabezado con "Itzyolotl" y el badge.
- `Pages/Home.razor` (portada con botón "Ver menú"), `Pages/Menu.razor` + `Menu.razor.cs` (code-behind).
- `Styles/app.tailwind.css`: `@import "tailwindcss"`, `@theme` (colores `obsidiana`, `superficie`, `borde`, `marca`, `texto-suave`) y capa base oscura.
- `wwwroot/index.html` (lang es, `theme-color`, enlaza `css/tailwind.css` y luego `css/app.css`, favicon `icon-192.png`), `wwwroot/css/app.css` (plantilla; aro de carga recolorado), `wwwroot/css/tailwind.css` (**generado y versionado a propósito**), `wwwroot/appsettings.json`.
- `.csproj`: target MSBuild `TailwindBuild` (antes de `BeforeBuild`) que ejecuta `tools/tailwindcss.exe`; minifica solo en Release.

**tests** — `Itzyolotl.Domain.Tests/Schedule/OpeningScheduleTests.cs` (9 pruebas).
**tools** — `README.md` con la descarga fijada de Tailwind 4.3.5; `tailwindcss.exe` no se sube a Git.

## 6. Configuración

- **Builds:** `Directory.Build.props` (Nullable, ImplicitUsings, EnforceCodeStyleInBuild; `TreatWarningsAsErrors` **solo en Release**). `Directory.Packages.props` con gestión central: los `.csproj` **no** llevan `Version`.
- **Paquetes (versiones en `Directory.Packages.props`):** `Microsoft.AspNetCore.OpenApi` 10.0.12; `Aspire.Hosting.AppHost` y `Aspire.Hosting.Dotnet` 17.0.0; `Aspire.Hosting.SqlServer` (alineado con 17.0.0 ⚠️); `Aspire.Microsoft.EntityFrameworkCore.SqlServer` 17.0.0 (en Infrastructure); `Microsoft.EntityFrameworkCore` 10.0.15 (Application); `Microsoft.EntityFrameworkCore.Design` 10.0.15 (Api, `PrivateAssets=all`); `Microsoft.Extensions.Http.Resilience` y `ServiceDiscovery` 10.10.0; OpenTelemetry 1.18.0 (Exporter.OpenTelemetryProtocol, Extensions.Hosting, Instrumentation.AspNetCore/Http/Runtime); `Microsoft.AspNetCore.Components.WebAssembly` y `.DevServer` 10.0.12; pruebas: `xunit` 2.9.3, `xunit.runner.visualstudio` 3.1.4, `Microsoft.NET.Test.Sdk` 17.14.1, `coverlet.collector` 6.0.4.
- **SQL Server (AppHost):** contenedor persistente con volumen de datos, puerto fijo **14333**, parámetro `sql-password` guardado como *user secret* (`Parameters:sql-password`; **nunca en el repositorio**), base `itzyolotl`. SSMS: servidor `localhost,14333`, login `sa`, marcar *Trust server certificate*.
- **Runtime de contenedores:** `ASPIRE_CONTAINER_RUNTIME=docker` en ambos perfiles de `src/Itzyolotl.AppHost/Properties/launchSettings.json`.
- **Puertos:** Api `https://localhost:7234` y `http://localhost:5243`; Web.Customer `https://localhost:7162` y `http://localhost:5246`.
- **CORS:** el AppHost entrega a la Api `Cors__AllowedOrigins__0/1` con los dos orígenes de Web.Customer. En `AppHost.cs`: `api` con `WithReference(db)`, `WithEnvironment(...)`, `WaitFor(db)`; `customer` con `AddDotnetProject(...)` y `WaitFor(api)`.
- **Cliente → API:** `ApiBaseUrl` = `https://localhost:7234` en `wwwroot/appsettings.json` (público; nunca poner secretos ahí).
- **Migraciones:** `dotnet ef migrations add <Nombre> --project src/Itzyolotl.Infrastructure --startup-project src/Itzyolotl.Api --output-dir Persistence/Migrations`. **Nunca** `dotnet ef database update` (la fábrica de diseño apunta a una base falsa); se aplican al arrancar la Api en Desarrollo.
- **Tailwind (sin npm):** `tools/tailwindcss.exe`, entrada `Styles/app.tailwind.css`, salida `wwwroot/css/tailwind.css`. Se regenera en cada compilación. Modo vigilancia, desde `src/Itzyolotl.Web.Customer`: `..\..\tools\tailwindcss.exe -i Styles/app.tailwind.css -o wwwroot/css/tailwind.css --watch`.
- **Datos reales:** zona horaria fija `America/Mexico_City` en `GetStoreStatusHandler`; no se ha comprobado en contenedores Linux (requiere base de zonas horarias).

## 7. Progreso

### 7.1 Terminado (✅)
- Solución `.slnx` con 7 proyectos en `src/` y proyecto de pruebas; configuración común y gestión central de paquetes.
- AppHost con SQL Server en Docker (persistente, volumen, puerto 14333); contenedor visible en Docker Desktop; conexión desde SSMS.
- ServiceDefaults activo en la Api; telemetría, trazas y métricas visibles en el dashboard; `/health`.
- Dominio: `Category`, `Product`, `OpeningPeriod`, `OpeningSchedule`.
- EF Core: contexto, configuraciones y 2 migraciones aplicadas (tablas `Categories`, `Products`, `OpeningPeriods`, `__EFMigrationsHistory`).
- Seeder de desarrollo (10 productos, 6 tramos).
- Endpoints `GET /api/menu` y `GET /api/store/status` (JSON verificado).
- 9 pruebas de la regla de apertura, todas pasan; se comprobó que el borde `<` vs `<=` en el cierre rompe una prueba.
- Web.Customer corriendo bajo el AppHost, con CORS funcionando, página `/menu`, Tailwind 4.3.5 integrado, tema oscuro, servicios con code-behind, `StoreStatusBadge`, layout con encabezado y portada.

### 7.2 Siguiente (nada de esto está implementado)
- **1.11:** mostrar el horario semanal (ya viene en `StoreStatusDto.WeeklySchedule`) en la portada; compartir el estado del local entre encabezado y portada para no pedir `/api/store/status` dos veces; pruebas automáticas de `SpanishText`. Con eso termina la Fase 1.
- Punto exacto en que se detuvo el trabajo: la sección **1.10e** quedó completa y confirmada ("todo sale bien"). No se escribió código de la 1.11.

### 7.3 Verificar primero (⚠️)
- Rama actual, lista de commits y si lo último está confirmado en Git (el script de §11 lo imprime).
- Que `DevelopmentDataSeeder.cs` use 19:00–23:00 y que la tabla `OpeningPeriods` se haya actualizado a ese horario (se indicó, no se confirmó).
- Versión de `Aspire.Hosting.SqlServer` en `Directory.Packages.props`.
- Archivos que el nuevo chat debe pedir antes de editar: `AppHost.cs`, `Api/Program.cs`, `Web.Customer/Program.cs`, `Menu.razor`, `Home.razor`, `MainLayout.razor`, `Directory.Packages.props`.

### 7.4 Pendiente del usuario
- Logo en SVG (ya lo tiene) y archivos de iconos: `favicon.ico`, PNG 192 y 512, PNG 180 (`apple-touch-icon`). El usuario avisará cuando estén listos; **no explicar cómo hacerlos**.
- Confirmar con el dueño: días y horas de apertura reales, menú y precios reales.

## 8. Plan

Funcionalidades previstas:
1. Estado abierto/cerrado y horario ✅ (falta mostrar el horario semanal).
2. Pedido desde el celular; el pedido entra a la cola **solo cuando el pago está confirmado**.
3. Tiempo de espera configurable: minutos de preparación por producto + fila. `espera = (suma de minutos de los pedidos pagados por delante ÷ capacidad) + tiempo del propio pedido`; la capacidad (pedidos en paralelo) la fija el administrador.
4. Ocupación de mesas con tiempo aproximado.
5. Inventario de insumos que se descuenta por pedido, con aviso de reposición.
6. Vista de cola para trabajadores, legible en celular o tablet, en tiempo real.
7. Corte del día: ingresos, costo estimado, gastos manuales, utilidad, más vendidos y horas pico.
8. Tres perfiles con permisos distintos.

Orden de implementación:
1. Base (casi terminada: falta 1.11).
2. Núcleo: entidad de pedido, estados, pedido del cliente, cola del trabajador en tiempo real, confirmación manual de pago. Aquí se crean `Web.Staff` y `UI`.
3. Tiempos: minutos de preparación, capacidad y espera estimada.
4. Roles: login, permisos y bitácora de cancelaciones.
5. Mesas con control manual y cronómetro.
6. Inventario con recetas.
7. Corte del día.
8. Pulido: pasarela de pago real, despliegue y diseño final.

Permisos:

| Acción | Cliente | Trabajador | Admin |
|---|---|---|---|
| Ver menú, horario, mesas | ✅ | ✅ | ✅ |
| Crear pedido | ✅ | ✅ (mostrador) | ✅ |
| Ver sus pedidos | ✅ | — | — |
| Cancelar pedido | solo antes de pagar | antes de preparar | siempre, con motivo |
| Confirmar pago, cambiar estado, marcar mesas | ❌ | ✅ | ✅ |
| Inventario | ❌ | solo lectura | edita |
| Editar menú, precios, tiempos, horario, usuarios | ❌ | ❌ | ✅ |
| Corte del día y ganancias | ❌ | ❌ | ✅ |

El trabajador no ve ganancias ni costos. Toda cancelación o ajuste queda en una bitácora.

## 9. Decisiones técnicas

- Monolito modular con capas; `Domain` sin dependencias; entidades con setters privados.
- Dos frontends separados (cliente liviano; panel con login) más librería `UI`.
- Ids `int` para catálogo; los pedidos usarán clave única de idempotencia (p. ej. `Guid` generado por el cliente) para que un reintento no duplique el pedido.
- Estados de pedido previstos: pendiente de pago → pagado (entra a la cola) → en preparación → listo → entregado; o cancelado.
- Pago: en el MVP lo confirma el trabajador (efectivo o transferencia); pasarela real al final.
- Inventario por **recetas**: insumo con presentación y costo, receta por producto, modificador "para llevar" que suma bolsa y tenedor. El rendimiento se calcula, no se escribe a mano. Descuento al confirmar el pago; se devuelve si se cancela antes de preparar.
- Mesas: control manual con hora de inicio; **cámaras descartadas** (complejidad y privacidad).
- Tiempo real con SignalR; autenticación con ASP.NET Core Identity y políticas por rol.
- Internet lento: caché con service worker (sin instalación), reintentos con `IHttpClientFactory` + resiliencia (pendiente de agregar el paquete), pedidos idempotentes, estado conservado ante fallos, `AsNoTracking` y caché del menú.
- Migraciones al arrancar solo en Desarrollo; antes de desplegar pasarán a un paso separado.
- `Web.Customer` se sirve de la forma clásica (proyecto normal en el AppHost + CORS). **Descartado el gateway de Aspire 17** (§10).
- `tailwind.css` generado se versiona (una compilación nueva no empaqueta archivos que aparecen durante la compilación). Revisar antes de desplegar.
- Variantes del producto (tamaños, sabores, rellenos) no están modeladas; se diseñan junto con los pedidos.
- No usar `DateTime.Now`: la hora entra por `TimeProvider`.

## 10. Problemas conocidos y soluciones

- **Gateway Blazor de Aspire 17 no arranca:** `Microsoft.AspNetCore.Components.Gateway.Cli 11.0.0` falla con "DotnetToolSettings.xml no se encontró" (incidencia `microsoft/aspire` #22387); hipótesis: exige .NET 11 y solo hay SDK 10. Se usa la vía clásica.
- **F5 lanza `OptionsValidationException` (CliPath/DashboardPath):** fallo intermitente de Aspire 17 en Visual Studio (#22415). Solución: cerrar otros editores con la solución y **Compilar → Recompilar solución**.
- **`AddServiceDefaults()` duplicado** en `Program.cs` provoca "Multiple calls to UseOtlpExporter". Debe llamarse una sola vez.
- **El contenedor SQL no aparecía en Docker Desktop:** Aspire había usado WSLc porque Visual Studio no veía `docker` en su PATH. Solución: reabrir Visual Studio y fijar `ASPIRE_CONTAINER_RUNTIME=docker`. Existe un contenedor viejo `sql-4d963a02` detenido en WSLc (sin borrar, con su volumen).
- **`dotnet add package` con gestión central:** si un `.csproj` nuevo trae `Version`, aparece `NU1008`; mover la versión a `Directory.Packages.props`.
- **Release trata las advertencias como errores:** compilar siempre también con `-c Release`. La integración experimental de Aspire para Blazor generaba `ASPIREBLAZOR001`.
- **El CSS de plantilla (`app.css`) no está en una capa de Tailwind:** gana sobre las utilidades. No hay reglas sobre `a`, `body` ni `h1`.
- **Clases de Tailwind:** el CLI lee el texto; nunca armar nombres de clase por partes.

## 11. Git y coordinación entre chats

Estado actual: ⚠️ no verificado (rama, commits y base los imprime el script). El chat 1 indicó commit al cerrar cada sección; el primer commit sugerido fue "Estructura inicial de la solucion".

Procedimiento propuesto (el usuario lo autoriza al aplicarlo):
1. `main` (o la rama estable actual) solo se toca con **autorización expresa**. Ningún chat fusiona.
2. Cada chat trabaja en su propia rama creada desde el último commit del anterior, por ejemplo `dev/chat-2`. Commits pequeños, uno por sección, con el número de sección en el mensaje, p. ej. `[1.11] horario semanal`.
3. **Un solo chat activo a la vez.** Si el chat 1 retoma el trabajo, el usuario primero le entrega el archivo exportado más reciente; nunca se edita en paralelo la misma rama.
4. Al terminar una sesión: commit → ejecutar `bash scripts/generar-contexto.sh` → compartir `contexto-para-otro-chat.txt` con el otro chat.
5. El script incluye este documento, el estado de Git y el contenido completo de los archivos cambiados o creados respecto a la rama base (confirmados o no), con secretos redactados. `--todo` incluye todo el proyecto (usarlo al iniciar un chat nuevo); `--base <ref>` fija la base.
6. Al cerrar cada sección, el chat activo propone la actualización de §7 de este archivo; el usuario la aplica y hace commit. Este archivo vive en `docs/contexto-proyecto.md`.
7. La fusión a la rama estable la hace el usuario cuando decide, nunca la IA por iniciativa propia.