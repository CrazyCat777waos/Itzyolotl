using Itzyolotl.Infrastructure;
using Itzyolotl.Application;
using Itzyolotl.Application.Menu;
using Itzyolotl.Application.Store;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddInfrastructure();

builder.Services.AddApplication();

builder.Services.AddOpenApi();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
    options.AddPolicy("Web", policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await app.ApplyMigrationsAsync(); //NO SUBIR A PRODUCCION
    await app.SeedDevelopmentDataAsync();
}

app.UseHttpsRedirection();

app.UseCors("Web");

app.MapGet("/api/menu", async (GetMenuHandler handler, CancellationToken cancellationToken) =>
        await handler.HandleAsync(cancellationToken))
    .WithName("GetMenu");
app.MapGet("/api/store/status", async (GetStoreStatusHandler handler, CancellationToken cancellationToken) =>
        await handler.HandleAsync(cancellationToken))
    .WithName("GetStoreStatus");

app.Run();