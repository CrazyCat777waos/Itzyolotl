var builder = DistributedApplication.CreateBuilder(args);

var sqlPassword = builder.AddParameter("sql-password", secret: true);

var sql = builder.AddSqlServer("sql", sqlPassword, port: 14333)
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

var db = sql.AddDatabase("itzyolotl");

builder.AddDotnetProject("api", "../Itzyolotl.Api/Itzyolotl.Api.csproj")
    .WithReference(db)
    .WaitFor(db);

builder.Build().Run();