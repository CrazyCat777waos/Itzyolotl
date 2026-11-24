using Itzyolotl.Infrastructure;
using Itzyolotl.Application;
using Itzyolotl.Application.Menu;
using Itzyolotl.Application.Store;
using Itzyolotl.Application.Orders;
using Itzyolotl.Contracts.Orders;

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
app.MapPost("/api/orders", async Task<IResult> (
        CreateOrderRequest request,
        CreateOrderHandler handler,
        CancellationToken cancellationToken) =>
{
    var result = await handler.HandleAsync(request, cancellationToken);

    return result.Outcome switch
    {
        CreateOrderOutcome.Created =>
            Results.Created($"/api/orders/{result.Order!.Id}", result.Order),
        CreateOrderOutcome.AlreadyExists =>
            Results.Ok(result.Order),
        _ =>
            Results.Problem(
                title: "Pedido no válido",
                detail: result.Error,
                statusCode: StatusCodes.Status400BadRequest)
    };
})
    .WithName("CreateOrder");

app.MapGet("/api/orders/{id:guid}", async Task<IResult> (
        Guid id,
        GetOrderHandler handler,
        CancellationToken cancellationToken) =>
{
    var order = await handler.HandleAsync(id, cancellationToken);

    return order is null ? Results.NotFound() : Results.Ok(order);
})
    .WithName("GetOrder");
app.Run();