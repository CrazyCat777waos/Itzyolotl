var builder = DistributedApplication.CreateBuilder(args);

var sqlPassword = builder.AddParameter("sql-password", secret: true);
var staffKey = builder.AddParameter("staff-key", secret: true);

var sql = builder.AddSqlServer("sql", sqlPassword, port: 14333)
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var db = sql.AddDatabase("itzyolotl");

var api = builder.AddDotnetProject("api", "../Itzyolotl.Api/Itzyolotl.Api.csproj")
    .WithEndpoint("https", endpoint => endpoint.Port = 7234)
    .WithEndpoint("http", endpoint => endpoint.Port = 5243)
    .WithReference(db)
    .WithEnvironment("Cors__AllowedOrigins__0", "https://localhost:7162")
    .WithEnvironment("Cors__AllowedOrigins__1", "http://localhost:5246")
    .WithEnvironment("Cors__AllowedOrigins__2", "https://localhost:7165")
    .WithEnvironment("Cors__AllowedOrigins__3", "http://localhost:5249")
    .WithEnvironment("Staff__AccessKey", staffKey)
    .WaitFor(db);

builder.AddDotnetProject("customer", "../Itzyolotl.Web.Customer/Itzyolotl.Web.Customer.csproj")
    .WithEndpoint("https", endpoint => endpoint.Port = 7162)
    .WithEndpoint("http", endpoint => endpoint.Port = 5246)
    .WaitFor(api);

builder.AddDotnetProject("staff", "../Itzyolotl.Web.Staff/Itzyolotl.Web.Staff.csproj")
    .WithEndpoint("https", endpoint => endpoint.Port = 7165)
    .WithEndpoint("http", endpoint => endpoint.Port = 5249)
    .WaitFor(api);

builder.Build().Run();