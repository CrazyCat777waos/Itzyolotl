using Itzyolotl.Infrastructure;
using Itzyolotl.Application;
using Itzyolotl.Application.Menu;
using Itzyolotl.Application.Store;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddInfrastructure();

builder.Services.AddApplication();

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await app.ApplyMigrationsAsync(); //NO SUBIR A PRODUCCION
    await app.SeedDevelopmentDataAsync();
}

app.UseHttpsRedirection();

app.MapGet("/api/menu", async (GetMenuHandler handler, CancellationToken cancellationToken) =>
        await handler.HandleAsync(cancellationToken))
    .WithName("GetMenu");
app.MapGet("/api/store/status", async (GetStoreStatusHandler handler, CancellationToken cancellationToken) =>
        await handler.HandleAsync(cancellationToken))
    .WithName("GetStoreStatus");

app.Run();