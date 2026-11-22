using Itzyolotl.Infrastructure;
using Itzyolotl.Application;
using Itzyolotl.Application.Menu;

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

app.Run();