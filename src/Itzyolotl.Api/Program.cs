using Itzyolotl.Infrastructure;
using Itzyolotl.Application;
using Itzyolotl.Application.Menu;
using Itzyolotl.Application.Store;
using Itzyolotl.Application.Orders;
using Itzyolotl.Contracts.Orders;
using Itzyolotl.Api.Hubs;
using Itzyolotl.Api.Security;
using Itzyolotl.Application.Abstractions;

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
        .AllowAnyMethod()
        .AllowCredentials()));

builder.Services.AddSignalR();
builder.Services.AddSingleton<IOrderNotifier, SignalROrderNotifier>();

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

app.MapHub<OrdersHub>("/hubs/orders");

var staff = app.MapGroup("/api/staff")
    .AddEndpointFilter(new StaffKeyFilter(app.Configuration["Staff:AccessKey"]));

staff.MapGet("/orders", async (GetStaffQueueHandler handler, CancellationToken cancellationToken) =>
        Results.Ok(await handler.HandleAsync(cancellationToken)))
    .WithName("GetStaffQueue");

staff.MapPost("/orders/{id:guid}/actions", async Task<IResult> (
        Guid id,
        OrderActionRequest request,
        ChangeOrderStatusHandler handler,
        CancellationToken cancellationToken) =>
{
    var result = await handler.HandleAsync(id, request, cancellationToken);

    return result.Outcome switch
    {
        ChangeOrderOutcome.Ok =>
            Results.Ok(result.Order),
        ChangeOrderOutcome.NotFound =>
            Results.NotFound(),
        ChangeOrderOutcome.Conflict =>
            Results.Problem(
                title: "Conflicto",
                detail: result.Error,
                statusCode: StatusCodes.Status409Conflict),
        _ =>
            Results.Problem(
                title: "Acción no válida",
                detail: result.Error,
                statusCode: StatusCodes.Status400BadRequest)
    };
})
    .WithName("ChangeOrderStatus");

app.Run();