var builder = DistributedApplication.CreateBuilder(args);

var sqlPassword = builder.AddParameter("sql-password", secret: true);

var sql = builder.AddSqlServer("sql", sqlPassword, port: 14333)
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var db = sql.AddDatabase("itzyolotl");

var api = builder.AddDotnetProject("api", "../Itzyolotl.Api/Itzyolotl.Api.csproj")
    .WithReference(db)
    .WithEnvironment("Cors__AllowedOrigins__0", "https://localhost:7162")
    .WithEnvironment("Cors__AllowedOrigins__1", "http://localhost:5246")
    .WaitFor(db);

builder.AddDotnetProject("customer", "../Itzyolotl.Web.Customer/Itzyolotl.Web.Customer.csproj")
    .WaitFor(api);

builder.Build().Run();