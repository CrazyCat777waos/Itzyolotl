# Itzyolotl — Contexto técnico para continuidad entre chats

Última actualización: 27/11/2026 (chat 2: cierre de las Fases 1 y 2). Quien avance el proyecto debe actualizar §7 al cerrar cada sección.

Leyenda: ✅ confirmado por el usuario con salida o resultado visible · ⚠️ indicado en el chat pero sin confirmación explícita · ❌ no existe todavía.

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

- El usuario aprende mientras construye: una sección a la vez, indicar **siempre el archivo**, explicar qué hace y por qué. Desde la Fase 2 acepta bloques más grandes (varios archivos por entrega); él avisa cuando algo se le complica o no lo entiende.
- Etiquetar conceptos: 🟢 conocido · 🟡 para estudiar · 🔴 avanzado. No evitar técnicas avanzadas; explicar para qué sirven y qué estudiar.
- Domina C#, .NET y SQL Server. **No asumir** dominio de HTML, CSS, Tailwind ni JavaScript.
- Responder en español. Pedir con comandos exactos las salidas o archivos a verificar, en lugar de adivinar. Verificar con `dotnet build -c Release` y `dotnet test -c Release` (el build Release trata las advertencias como errores); el usuario hace commit al cerrar cada sección.
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
| Tiempo real | SignalR: el servidor viene con ASP.NET Core; cliente `Microsoft.AspNetCore.SignalR.Client` 10.0.15 en `Web.Staff` |
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
   +---- UI  <---- Web.Customer, Web.Staff (ambos también conocen Contracts)
AppHost orquesta: sql -> itzyolotl (db) -> api -> customer, staff
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
| `Web.Customer` | Frontend público (Blazor WASM): menú, carrito y seguimiento del pedido | Contracts, UI |
| `Web.Staff` | Panel del personal (Blazor WASM): cola de pedidos en tiempo real. Acceso con clave temporal (login real en la Fase 4) | Contracts, UI |
| `UI` | Biblioteca de clases compartida por los frontends. Por ahora solo formato de texto (`SpanishText`); pasará a Razor Class Library cuando se compartan componentes | Contracts |
| `tests/Domain.Tests` | Pruebas xUnit de Domain (y de que los límites de `Contracts` coincidan con los del dominio) | Domain, Contracts |
| `tests/UI.Tests` | Pruebas xUnit de `SpanishText` | UI |

Dos frontends separados (✅ creados en las Fases 1 y 2): `Web.Customer` y `Web.Staff` (trabajadores y administrador; el login real llega en la Fase 4), más la biblioteca `UI`. Motivo de separar: el celular del cliente no debe descargar el código del panel de gestión.

## 5. Estructura y archivos importantes

Raíz `C:\Dev\Itzyolotl`: `Itzyolotl.slnx`, `Directory.Build.props`, `Directory.Packages.props`, `dotnet-tools.json`, `.gitignore`, `.editorconfig`, `tools/`, `src/`, `tests/`, `docs/`, `scripts/`.

**Domain**
- `Common/Entity.cs`: `int Id { get; protected set; }`.
- `Entities/Category.cs`: `Name`, `DisplayOrder`; `Rename`.
- `Entities/Product.cs`: `CategoryId`, `Name`, `Description?`, `Price` (decimal), `PreparationMinutes`, `IsAvailable`; `ChangePrice`, `SetAvailability`. Setters privados, validación en constructor.
- `Entities/OpeningPeriod.cs`: `Day` (DayOfWeek), `OpensAt`, `ClosesAt` (TimeOnly). Exige cierre > apertura (**no soporta cierre pasada la medianoche**).
- `Schedule/OpeningSchedule.cs`: función pura `Evaluate(periods, localNow)` → `StoreStatus(IsOpen, NextChangeLocal)`. Abierto si `abre <= ahora < cierra`.
- `Entities/Order.cs`: `IdempotencyKey` (`Guid`, clave única), `CustomerName` (máx. 60), `ServiceType`, `Status`, `CreatedAt`, una marca de tiempo por paso (`PaidAt`, `PreparationStartedAt`, `ReadyAt`, `DeliveredAt`, `CancelledAt`), `CancellationReason` (máx. 200), `RowVersion` (concurrencia optimista), `Lines` (solo lectura) y `Total` (calculado). Constructor público validado y constructor privado sin parámetros solo para EF. Métodos `MarkPaid`, `StartPreparation`, `MarkReady`, `MarkDelivered` y `Cancel(reason, now)`: rechazan saltos de estado con `InvalidOperationException`; solo se cancela desde `PendingPayment` o `Paid`. La hora entra por parámetro.
- `Entities/OrderLine.cs`: `ProductId`, `ProductName` y `UnitPrice` **copiados** del producto al pedir, `Quantity`, `Note?` (máx. 200; se recorta y si queda vacía es `null`) y `Subtotal` calculado.
- `Entities/OrderStatus.cs` y `Entities/ServiceType.cs`: enums con valores numéricos **fijos** (EF los guarda como entero y `GetStaffQueueHandler` compara `Status < Delivered`); no reordenar.

**Application**
- `Abstractions/IAppDbContext.cs`: `DbSet` de `Categories`, `Products`, `OpeningPeriods` y `Orders`, más `SaveChangesAsync`.
- `Abstractions/IOrderNotifier.cs`: `NotifyQueueChangedAsync`; la implementación vive en `Api` (Application no conoce SignalR).
- `Menu/GetMenuHandler.cs`: una consulta con `join`, `AsNoTracking`, agrupa por categoría en memoria.
- `Store/GetStoreStatusHandler.cs`: usa `TimeProvider` y la zona fija `America/Mexico_City`; devuelve estado, próximo cambio (`DateTimeOffset` con desfase), el día local de la tienda (`Today`) y horario semanal de los 7 días.
- `Orders/`: `CreateOrderHandler` (valida la solicitud; idempotente por `IdempotencyKey`; busca los productos en una sola consulta; copia nombre y precio; si dos peticiones iguales chocan con el índice único devuelve el pedido existente; avisa a la cola), `GetOrderHandler` (por `Guid`), `GetStaffQueueHandler` (pedidos con `Status < Delivered`, ordenados por `PaidAt ?? CreatedAt`), `ChangeOrderStatusHandler` (aplica la acción en el dominio; responde conflicto por transición inválida o por `DbUpdateConcurrencyException`), `CreateOrderResult`, `ChangeOrderStatusResult` y `OrderMapping`.
- `DependencyInjection.cs`: `AddApplication()` (`TryAddSingleton(TimeProvider.System)`, handlers `Scoped`).

**Contracts**
- `Menu/MenuDto.cs`: `MenuDto`, `MenuCategoryDto`, `MenuProductDto`.
- `Store/StoreStatusDto.cs`: `StoreStatusDto(IsOpen, NextChangeAt, TimeZoneId, Today, WeeklySchedule)`, `DayScheduleDto`, `OpeningPeriodDto`.
- `Orders/`: `CreateOrderRequest` (+ `CreateOrderLineRequest`, `ServiceTypeDto`; solo `ProductId` y `Quantity`, **nunca precios**), `OrderDto` (+ `OrderLineDto`, `OrderStatusDto`; `Id` es el `Guid` público y `Number` el número visible), `OrderActionRequest` (+ `OrderAction`), `OrderLimits` (límites compartidos con los frontends) y `OrdersHubNames` (ruta y nombres del hub).

**Infrastructure**
- `Persistence/AppDbContext.cs` (implementa `IAppDbContext`, `ApplyConfigurationsFromAssembly`).
- `Persistence/Configurations/` (`Category`, `Product`, `OpeningPeriod`, `Order`, `OrderLine`). `Order`: índice único en `IdempotencyKey`, índice `(Status, CreatedAt)`, `RowVersion`, límites tomados de las constantes del dominio y líneas en cascada. `OrderLine`: precio `decimal(10,2)` y relación con `Product` con `Restrict` (un producto con pedidos no se borra; se retira con `IsAvailable`).
- `Persistence/DesignTimeDbContextFactory.cs`: cadena falsa solo para que `dotnet ef` lea el modelo.
- `Persistence/Migrations/`: `InitialCreate`, `AddOpeningPeriods`, `AddOrders`, `AddOrderRowVersion`.
- `DependencyInjection.cs`: `AddInfrastructure()` (`AddSqlServerDbContext<AppDbContext>("itzyolotl")` y `IAppDbContext` scoped) y `ApplyMigrationsAsync(IHost)`.
- `DevelopmentDataSeeder.cs`: `SeedDevelopmentDataAsync`, idempotente por tabla. Siembra 5 categorías (Cafés, Crepas saladas, Crepas dulces, Frappes, Paninis), 10 productos de ejemplo y tramos lunes–sábado de 19:00 a 23:00 (comprobado en el código).

**Api** — `Program.cs`: `AddServiceDefaults()` (**una sola vez**), `AddInfrastructure()`, `AddApplication()`, `AddOpenApi()`, política CORS `"Web"` leída de `Cors:AllowedOrigins` (con `AllowCredentials()`, necesario para SignalR; los orígenes son explícitos), `AddSignalR()`, `IOrderNotifier` → `SignalROrderNotifier`, `MapDefaultEndpoints()`, `UseCors("Web")`. En Desarrollo: `MapOpenApi`, `ApplyMigrationsAsync`, `SeedDevelopmentDataAsync`. Endpoints públicos: `GET /api/menu`, `GET /api/store/status`, `POST /api/orders` (201 creado, 200 si ya existía, 400 inválido) y `GET /api/orders/{guid}`. Hub `/hubs/orders` (`OrdersHub`: método `JoinStaff`, evento `QueueChanged`, **sin datos**). Grupo `/api/staff` protegido por `StaffKeyFilter` (clave temporal en el encabezado `X-Staff-Key`, comparada en tiempo constante; 401 si no coincide y 503 si no hay clave configurada): `GET /api/staff/orders` (cola de pedidos activos) y `POST /api/staff/orders/{guid}/actions` (cobrar, preparar, marcar listo, entregar o cancelar con motivo; 409 en conflicto).

**ServiceDefaults** — `Extensions.cs`: `AddServiceDefaults`, `ConfigureOpenTelemetry`, `AddDefaultHealthChecks`, `MapDefaultEndpoints` (`/health`, `/alive` en Desarrollo).

**AppHost** — `AppHost.cs` (no existe `Program.cs`). Declara los parámetros secretos `sql-password` y `staff-key` (este llega a la Api como `Staff__AccessKey`) y los proyectos `api`, `customer` y `staff`.

**Web.Customer**
- `Program.cs`: lee `ApiBaseUrl` de `wwwroot/appsettings.json`; registra `HttpClient`, `MenuApiClient`, `StoreApiClient`, `StoreStatusState`, `OrdersApiClient` y `CartState` (scoped).
- `Services/MenuApiClient.cs`, `Services/StoreApiClient.cs`, `Services/OrdersApiClient.cs`: únicos que conocen las rutas de la API (el de pedidos distingue 400 y 404 sin lanzar excepción).
- `Services/StoreStatusState.cs`: estado del local compartido entre componentes; actualiza cada 60 s y conserva el último estado válido si falla.
- `Services/CartState.cs` y `CartItem.cs`: carrito guardado en `localStorage` (llamado con `IJSRuntime`, sin JavaScript propio), una línea por producto, clave de idempotencia por intento de envío (se renueva si cambia el carrito), nombre y modalidad recordados, último pedido recordado.
- El formato de texto en español (`SpanishText`) **se movió a `Itzyolotl.UI`**; `Web.Customer` ya no tiene carpeta `Formatting`.
- `Components/StoreStatusBadge.razor(.cs)`: punto verde/rojo, "Abierto/Cerrado", próxima hora; lee el estado compartido de `StoreStatusState`; `IDisposable`.
- `Components/WeeklySchedule.razor(.cs)`: los siete días de lunes a domingo, "Cerrado" donde no hay tramos y el día actual resaltado.
- `Components/CartLink.razor(.cs)`: enlace "Carrito" con el contador; `IDisposable`.
- `Layout/MainLayout.razor`: encabezado con "Itzyolotl", el enlace al carrito y el badge.
- `Pages/Home.razor` (portada con botón "Ver menú" y horario semanal), `Pages/Menu.razor` + `Menu.razor.cs` (botón "Agregar" por producto y barra inferior con el total).
- `Pages/ShoppingCart.razor(.cs)` (`/carrito`: cantidades, nota por producto, nombre y modalidad, envío) y `Pages/OrderTracking.razor(.cs)` (`/pedido/{guid}`: estado, etapas y total; consulta cada 10 s hasta que el pedido esté entregado o cancelado). La clase se llama `ShoppingCart` porque un miembro no puede llamarse igual que su clase.
- `Styles/app.tailwind.css`: `@import "tailwindcss"`, `@theme` (colores `obsidiana`, `superficie`, `borde`, `marca`, `texto-suave`) y capa base oscura.
- `wwwroot/index.html` (lang es, `theme-color`, enlaza `css/tailwind.css` y luego `css/app.css`, favicon `icon-192.png`), `wwwroot/css/app.css` (plantilla; aro de carga recolorado), `wwwroot/css/tailwind.css` (**generado y versionado a propósito**), `wwwroot/appsettings.json`.
- `.csproj`: target MSBuild `TailwindBuild` (antes de `BeforeBuild`) que ejecuta `tools/tailwindcss.exe`; minifica solo en Release.

**Web.Staff** (puertos 7165 y 5249)
- `Program.cs`: lee `ApiBaseUrl`; registra `HttpClient`, `StaffAccess`, `StaffApiClient` y `StaffQueueState`.
- `Services/StaffAccess.cs`: clave temporal guardada en `localStorage` (`itzyolotl.staff-key`). **Se retira en la Fase 4.**
- `Services/StaffApiClient.cs`: envía `X-Staff-Key`; lanza `UnauthorizedAccessException` ante un 401.
- `Services/StaffQueueState.cs`: lista de pedidos, conexión SignalR (reintento cada 5 s sin límite; al reconectar **vuelve a unirse al grupo `staff`**), consulta de respaldo cada 30 s y un contador que descarta respuestas atrasadas.
- `Layout/StaffLayout.razor(.cs)`: encabezado con el indicador "En vivo"/"Reconectando" y "Salir"; muestra `Components/AccessForm.razor(.cs)` si no hay clave.
- `Pages/OrderQueue.razor(.cs)`: cuatro columnas (Por cobrar, En cola, Preparando, Listos) con una acción principal por columna; cancelar con motivo solo en las dos primeras.
- `Styles/app.tailwind.css` (misma paleta que Web.Customer), `wwwroot/index.html` (con `noindex`), `.csproj` con el mismo target `TailwindBuild`.

**UI** — `Formatting/SpanishText.cs`: `DayName`, `Capitalized`, `Range`, `Time`, `NextChange`, `OrderStatusLabel`, `ServiceTypeLabel` y `Elapsed`. Biblioteca de clases (`Microsoft.NET.Sdk`) que solo referencia Contracts.

**tests** — `Itzyolotl.Domain.Tests`: `Schedule/OpeningScheduleTests.cs` (9), `Orders/OrderLineTests.cs` (15), `Orders/OrderTests.cs` (20) y `Orders/OrderLimitsTests.cs` (1; exige que los límites de `Contracts` coincidan con los del dominio). `Itzyolotl.UI.Tests`: `Formatting/SpanishTextTests.cs` (45). Total **90** pruebas (contadas en el código; ver §7.3).
**tools** — `README.md` con la descarga fijada de Tailwind 4.3.5; `tailwindcss.exe` no se sube a Git.

## 6. Configuración

- **Builds:** `Directory.Build.props` (Nullable, ImplicitUsings, EnforceCodeStyleInBuild; `TreatWarningsAsErrors` **solo en Release**). `Directory.Packages.props` con gestión central: los `.csproj` **no** llevan `Version`.
- **Paquetes (versiones en `Directory.Packages.props`):** `Microsoft.AspNetCore.OpenApi` 10.0.12; `Aspire.Hosting.AppHost` y `Aspire.Hosting.Dotnet` 17.0.0; `Aspire.Hosting.SqlServer` 17.0.0; `Aspire.Microsoft.EntityFrameworkCore.SqlServer` 17.0.0 (en Infrastructure); `Microsoft.EntityFrameworkCore` 10.0.15 (Application); `Microsoft.EntityFrameworkCore.Design` 10.0.15 (Api, `PrivateAssets=all`); `Microsoft.Extensions.Http.Resilience` y `ServiceDiscovery` 10.10.0; OpenTelemetry 1.18.0 (Exporter.OpenTelemetryProtocol, Extensions.Hosting, Instrumentation.AspNetCore/Http/Runtime); `Microsoft.AspNetCore.Components.WebAssembly` y `.DevServer` 10.0.12; `Microsoft.AspNetCore.SignalR.Client` 10.0.15 (Web.Staff); pruebas: `xunit` 2.9.3, `xunit.runner.visualstudio` 3.1.4, `Microsoft.NET.Test.Sdk` 17.14.1, `coverlet.collector` 6.0.4.
- **SQL Server (AppHost):** contenedor persistente con volumen de datos, puerto fijo **14333**, parámetro `sql-password` guardado como *user secret* (`Parameters:sql-password`; **nunca en el repositorio**), base `itzyolotl`. SSMS: servidor `localhost,14333`, login `sa`, marcar *Trust server certificate*.
- **Clave temporal del personal:** parámetro secreto `staff-key` del AppHost (user secret `Parameters:staff-key` en el proyecto AppHost; **nunca en el repositorio**). Llega a la Api como `Staff__AccessKey` y los trabajadores la escriben en `Web.Staff`. **Se retira en la Fase 4.**
- **Runtime de contenedores:** `ASPIRE_CONTAINER_RUNTIME=docker` en ambos perfiles de `src/Itzyolotl.AppHost/Properties/launchSettings.json`.
- **Puertos:** Api `https://localhost:7234` y `http://localhost:5243`; Web.Customer `https://localhost:7162` y `http://localhost:5246`; Web.Staff `https://localhost:7165` y `http://localhost:5249`.
- **CORS:** el AppHost entrega a la Api `Cors__AllowedOrigins__0` a `__3`: los orígenes de Web.Customer (0 y 1) y de Web.Staff (2 y 3). En `AppHost.cs`: `api` con `WithReference(db)`, `WithEnvironment(...)`, `WaitFor(db)`; `customer` y `staff` con `AddDotnetProject(...)` y `WaitFor(api)`.
- **Cliente → API:** `ApiBaseUrl` = `https://localhost:7234` en `wwwroot/appsettings.json` (público; nunca poner secretos ahí).
- **Migraciones:** `dotnet ef migrations add <Nombre> --project src/Itzyolotl.Infrastructure --startup-project src/Itzyolotl.Api --output-dir Persistence/Migrations`. **Nunca** `dotnet ef database update` (la fábrica de diseño apunta a una base falsa); se aplican al arrancar la Api en Desarrollo.
- **Tailwind (sin npm):** `tools/tailwindcss.exe`, entrada `Styles/app.tailwind.css`, salida `wwwroot/css/tailwind.css`. Se regenera en cada compilación. Modo vigilancia, desde `src/Itzyolotl.Web.Customer` (igual en `src/Itzyolotl.Web.Staff`): `..\..\tools\tailwindcss.exe -i Styles/app.tailwind.css -o wwwroot/css/tailwind.css --watch`.
- **Datos reales:** zona horaria fija `America/Mexico_City` en `GetStoreStatusHandler`; no se ha comprobado en contenedores Linux (requiere base de zonas horarias).

## 7. Progreso

### 7.1 Terminado (✅)
- Solución `.slnx` con 10 proyectos en `src/` y 2 de pruebas; configuración común y gestión central de paquetes.
- AppHost con SQL Server en Docker (persistente, volumen, puerto 14333); contenedor visible en Docker Desktop; conexión desde SSMS.
- ServiceDefaults activo en la Api; telemetría, trazas y métricas visibles en el dashboard; `/health`.
- Dominio: `Category`, `Product`, `OpeningPeriod`, `OpeningSchedule`.
- EF Core: contexto, configuraciones y 4 migraciones aplicadas (tablas `Categories`, `Products`, `OpeningPeriods`, `Orders`, `OrderLines`, `__EFMigrationsHistory`).
- Seeder de desarrollo (10 productos, 6 tramos).
- Endpoints `GET /api/menu` y `GET /api/store/status` (JSON verificado).
- 9 pruebas de la regla de apertura, todas pasan; se comprobó que el borde `<` vs `<=` en el cierre rompe una prueba.
- Web.Customer corriendo bajo el AppHost, con CORS funcionando, página `/menu`, Tailwind 4.3.5 integrado, tema oscuro, servicios con code-behind, `StoreStatusBadge`, layout con encabezado y portada.
- **Fase 1 cerrada (1.11):** estado del local compartido entre encabezado y portada (`StoreStatusState`), horario semanal con el día actual resaltado y pruebas de `SpanishText`.
- **Fase 2 (núcleo de pedidos), probada de punta a punta por el usuario:**
  - Dominio: `Order`, `OrderLine`, `OrderStatus`, `ServiceType`; transiciones validadas, hora por parámetro, límites (nombre 60, nota 200, motivo 200) y `RowVersion`. 34 pruebas de dominio más la de límites compartidos.
  - EF Core: tablas `Orders` y `OrderLines` (migraciones `AddOrders` y `AddOrderRowVersion`); índice único en `IdempotencyKey`; índice `(Status, CreatedAt)`.
  - API del cliente: `POST /api/orders` idempotente (201 al crear, 200 si ya existía, 400 inválido; el precio lo pone siempre el servidor) y `GET /api/orders/{guid}`. Comprobado con PowerShell: 201, 200 al repetir, 400 con cantidad 0 y un solo pedido en SQL Server.
  - API del personal (`/api/staff`, clave temporal `X-Staff-Key`): cola de pedidos activos y acciones de cobrar, preparar, marcar listo, entregar y cancelar con motivo; 401, 409 y 400 comprobados a mano.
  - SignalR: hub `/hubs/orders`, grupo `staff`; solo avisa `QueueChanged`, sin datos.
  - `Web.Customer`: carrito persistente, envío con clave de idempotencia por intento y seguimiento del pedido.
  - `Web.Staff`: acceso con clave y cola en cuatro columnas en tiempo real. Comprobado en el navegador (8 puntos): clave incorrecta y correcta, pedido nuevo que aparece sin recargar, cobrar/preparar/listo/entregar, cancelar con motivo, **reconexión** tras reiniciar la Api y **choque entre dos trabajadores** (mensaje de conflicto).
  - Librería `Itzyolotl.UI` (`SpanishText`) y `Itzyolotl.UI.Tests`; `Elapsed` ("hace 5 min") con pruebas.
- Git: todo confirmado en `main` (último commit `91112cd`, árbol limpio).

### 7.2 Siguiente (nada de esto está implementado)
- **Fase 3 (tiempos):** minutos de preparación por producto (`Product.PreparationMinutes` ya existe), capacidad (pedidos en paralelo, la fija el administrador) y espera estimada con la fórmula de §8. Para medir la espera desde el pago habrá que agregar `PaidAt` a `OrderDto`.
- Punto exacto en que se detuvo el trabajo: la Fase 2 quedó completa, probada y confirmada en `main` (commit `91112cd`). No se escribió código de la Fase 3.

### 7.3 Pendientes técnicos y de seguridad (⚠️)
- **Retirar la clave temporal del personal** (`X-Staff-Key`, parámetro `staff-key` del AppHost, `StaffKeyFilter`, `StaffAccess`) en la Fase 4 y reemplazarla por Identity con políticas por rol. **No desplegar con ella.** Revisar entonces `AllowCredentials()` en CORS.
- **Pruebas automáticas de los handlers de pedidos** (idempotencia, carrera, conflictos): no existen. `RowVersion` impide usar SQLite en memoria; usar Testcontainers con SQL Server.
- Decidir si se permiten pedidos con el local cerrado (hoy se permiten).
- El cliente consulta el estado cada 10 s; evaluar SignalR por pedido si la carga lo justifica.
- Quitar de `Directory.Packages.props` la versión huérfana de `Aspire.Hosting.Blazor` (resto del gateway descartado; ningún `.csproj` la usa).
- Borrar `src/Itzyolotl.Web.Staff/wwwroot/sample-data/weather.json`: resto de la plantilla de Blazor, sin uso.
- Agregar resiliencia HTTP con `IHttpClientFactory`.
- El usuario no pegó la salida de `dotnet test -c Release` posterior al refactor de `UI`; las 90 pruebas están contadas en el código, pendientes de confirmar con la salida.

### 7.4 Pendiente del usuario
- Logo en SVG (ya lo tiene) y archivos de iconos: `favicon.ico`, PNG 192 y 512, PNG 180 (`apple-touch-icon`). El usuario avisará cuando estén listos; **no explicar cómo hacerlos**.
- Confirmar con el dueño: días y horas de apertura reales, menú y precios reales.

## 8. Plan

Funcionalidades previstas:
1. Estado abierto/cerrado y horario ✅ (también el horario semanal en la portada).
2. Pedido desde el celular; el pedido entra a la cola **solo cuando el pago está confirmado** ✅ (el cliente pide y sigue su pedido; el trabajador confirma el pago).
3. Tiempo de espera configurable: minutos de preparación por producto + fila. `espera = (suma de minutos de los pedidos pagados por delante ÷ capacidad) + tiempo del propio pedido`; la capacidad (pedidos en paralelo) la fija el administrador.
4. Ocupación de mesas con tiempo aproximado.
5. Inventario de insumos que se descuenta por pedido, con aviso de reposición.
6. Vista de cola para trabajadores, legible en celular o tablet, en tiempo real ✅ (primera versión en `Web.Staff`).
7. Corte del día: ingresos, costo estimado, gastos manuales, utilidad, más vendidos y horas pico.
8. Tres perfiles con permisos distintos.

Orden de implementación:
1. Base ✅ (Fase 1 cerrada).
2. ✅ Núcleo: entidad de pedido, estados, pedido del cliente, cola del trabajador en tiempo real, confirmación manual de pago. Aquí se crearon `Web.Staff` y `UI`.
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

Hoy (Fase 2): el personal entra con una clave temporal compartida (§6); la cancelación exige motivo y queda guardada en el propio pedido, pero todavía no hay roles ni bitácora (Fase 4).

## 9. Decisiones técnicas

- Monolito modular con capas; `Domain` sin dependencias; entidades con setters privados.
- Dos frontends separados (cliente liviano; panel con login) más librería `UI`: creados en la Fase 2; el login llega en la Fase 4.
- Ids `int` para catálogo; los pedidos llevan una clave única de idempotencia (`Guid` generado por el cliente) para que un reintento no duplique el pedido ✅.
- Estados de pedido (implementados): pendiente de pago → pagado (entra a la cola) → en preparación → listo → entregado; o cancelado.
- Pago: en el MVP lo confirma el trabajador (efectivo o transferencia); pasarela real al final.
- Inventario por **recetas**: insumo con presentación y costo, receta por producto, modificador "para llevar" que suma bolsa y tenedor. El rendimiento se calcula, no se escribe a mano. Descuento al confirmar el pago; se devuelve si se cancela antes de preparar.
- Mesas: control manual con hora de inicio; **cámaras descartadas** (complejidad y privacidad).
- Tiempo real con SignalR (✅ en la cola del personal; el cliente consulta cada 10 s); autenticación con ASP.NET Core Identity y políticas por rol (Fase 4).
- Internet lento: caché con service worker (sin instalación), reintentos con `IHttpClientFactory` + resiliencia (pendiente de agregar el paquete), pedidos idempotentes, estado conservado ante fallos, `AsNoTracking` y caché del menú.
- Migraciones al arrancar solo en Desarrollo; antes de desplegar pasarán a un paso separado.
- `Web.Customer` se sirve de la forma clásica (proyecto normal en el AppHost + CORS). **Descartado el gateway de Aspire 17** (§10).
- `tailwind.css` generado se versiona, el de `Web.Customer` y el de `Web.Staff` (una compilación nueva no empaqueta archivos que aparecen durante la compilación). Revisar antes de desplegar.
- Variantes del producto (tamaños, sabores, rellenos) **fuera del MVP**: cada crepa o panini es un producto y la línea del pedido lleva una **nota libre** opcional (máx. 200) escrita por el cliente. Se podrán agregar después sin romper lo anterior.
- No usar `DateTime.Now`: la hora entra por `TimeProvider`.
- El cliente **nunca envía precios**: manda `ProductId` y `Quantity`; el servidor copia nombre y precio a la línea del pedido, así cambiar el menú no altera pedidos ya hechos.
- El identificador público de un pedido es su `Guid` (el mismo de la idempotencia); el `Id` numérico solo se muestra como número de pedido. Así no se pueden recorrer pedidos ajenos.
- Una línea por producto en el carrito (la nota aplica a todas sus unidades); el dominio sí admite varias líneas del mismo producto con notas distintas.
- La clave de idempotencia se genera una vez por intento de envío y se renueva cuando cambia el carrito.
- Los límites (`OrderLimits`) viven en `Contracts` y una prueba exige que coincidan con los del dominio; el dominio valida lo que la base de datos limita.
- Concurrencia optimista con `RowVersion`: si dos trabajadores cambian el mismo pedido, el segundo recibe 409.
- El hub de SignalR solo avisa "algo cambió"; los datos viajan por la API protegida. Los avisos (`IOrderNotifier`) son de mejor esfuerzo: un aviso fallido no deshace un cambio ya guardado.
- Al reconectar SignalR hay que volver a unirse al grupo `staff`; además la cola se consulta cada 30 s como respaldo.
- Clave temporal del personal solo como puente hasta la Fase 4 (§6, §7.3).
- `Itzyolotl.UI` es una biblioteca de clases normal; pasará a Razor Class Library cuando se compartan componentes.

## 10. Problemas conocidos y soluciones

- **Gateway Blazor de Aspire 17 no arranca:** `Microsoft.AspNetCore.Components.Gateway.Cli 11.0.0` falla con "DotnetToolSettings.xml no se encontró" (incidencia `microsoft/aspire` #22387); hipótesis: exige .NET 11 y solo hay SDK 10. Se usa la vía clásica.
- **F5 lanza `OptionsValidationException` (CliPath/DashboardPath):** fallo intermitente de Aspire 17 en Visual Studio (#22415). Solución: cerrar otros editores con la solución y **Compilar → Recompilar solución**.
- **`AddServiceDefaults()` duplicado** en `Program.cs` provoca "Multiple calls to UseOtlpExporter". Debe llamarse una sola vez.
- **El contenedor SQL no aparecía en Docker Desktop:** Aspire había usado WSLc porque Visual Studio no veía `docker` en su PATH. Solución: reabrir Visual Studio y fijar `ASPIRE_CONTAINER_RUNTIME=docker`. Existe un contenedor viejo `sql-4d963a02` detenido en WSLc (sin borrar, con su volumen).
- **`dotnet add package` con gestión central:** si un `.csproj` nuevo trae `Version`, aparece `NU1008`; mover la versión a `Directory.Packages.props`.
- **Release trata las advertencias como errores:** compilar siempre también con `-c Release`. La integración experimental de Aspire para Blazor generaba `ASPIREBLAZOR001`.
- **El CSS de plantilla (`app.css`) no está en una capa de Tailwind:** gana sobre las utilidades. No hay reglas sobre `a`, `body` ni `h1`.
- **Clases de Tailwind:** el CLI lee el texto; nunca armar nombres de clase por partes.
- **`tailwind.css` cambia solo con compilar en Release** (el build Release lo minifica). Antes de confirmar, compilar en Debug para que el archivo versionado quede sin minificar y con las clases nuevas.
- **La primera vez que se compila un frontend nuevo** (como `Web.Staff`), el CSS recién generado no se empaqueta hasta la segunda compilación.
- **Tras reconectar, SignalR no devuelve al cliente a sus grupos:** sin volver a unirse, la pantalla parece conectada pero no recibe avisos.
- **`Assert.Throws<T>` exige el tipo exacto** de excepción, no uno derivado.
- **Con PowerShell clásico**, `$_.ErrorDetails.Message` suele venir vacío en errores HTTP; leer el cuerpo con `GetResponseStream()`.
- **Reemplazos de texto masivos desde PowerShell clásico** pueden regrabar archivos sin UTF-8 y estropear las tildes; editar a mano.
- **Un miembro no puede llamarse igual que su clase** (error CS0542): por eso la página del carrito es `ShoppingCart` y no `Cart`.

## 11. Git y coordinación entre chats

Estado actual (verificado el 27/11/2026 con el script): rama `main` igual a `origin/main`, último commit `91112cd` ("Creacion del proyecto UI para configuraciones del texto"), árbol limpio. El trabajo de las Fases 1 y 2 se hizo y confirmó directamente en `main`; no se usó la rama `dev/chat-2` del procedimiento propuesto. El primer commit del proyecto fue "Estructura inicial de la solucion".

Procedimiento propuesto (el usuario lo autoriza al aplicarlo):
1. `main` (o la rama estable actual) solo se toca con **autorización expresa**. Ningún chat fusiona.
2. Cada chat trabaja en su propia rama creada desde el último commit del anterior, por ejemplo `dev/chat-2`. Commits pequeños, uno por sección, con el número de sección en el mensaje, p. ej. `[1.11] horario semanal`.
3. **Un solo chat activo a la vez.** Si el chat 1 retoma el trabajo, el usuario primero le entrega el archivo exportado más reciente; nunca se edita en paralelo la misma rama.
4. Al terminar una sesión: commit → ejecutar `bash scripts/generar-contexto.sh` → compartir `contexto-para-otro-chat.txt` con el otro chat.
5. El script incluye este documento, el estado de Git y el contenido completo de los archivos cambiados o creados respecto a la rama base (confirmados o no), con secretos redactados. `--todo` incluye todo el proyecto (usarlo al iniciar un chat nuevo); `--base <ref>` fija la base.
6. Al cerrar cada sección, el chat activo propone la actualización de §7 de este archivo; el usuario la aplica y hace commit. Este archivo vive en `docs/contexto-proyecto.md`.
7. La fusión a la rama estable la hace el usuario cuando decide, nunca la IA por iniciativa propia.