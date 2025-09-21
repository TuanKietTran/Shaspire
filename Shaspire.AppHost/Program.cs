var builder = DistributedApplication.CreateBuilder(args);

// SQL Server with persistent lifetime
var sqlServer = builder.AddSqlServer("shaspire-sql")
    .WithLifetime(ContainerLifetime.Persistent);

var db = sqlServer.AddDatabase("Shaspire");

// PostgreSQL with persistent lifetime
var postgres = builder.AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent);

var postgresDb = postgres.AddDatabase("ShaspirePostgres");

// Redis cache with persistent lifetime
var redis = builder.AddRedis("redis")
    .WithLifetime(ContainerLifetime.Persistent);

// API Service with references to all databases
var apiService = builder.AddProject<Projects.Shaspire_ApiService>("apiservice")
    .WithReference(db)
    .WithReference(postgresDb)
    .WithReference(redis)
    .WaitFor(db)
    .WaitFor(postgresDb)
    .WaitFor(redis);

// Web Frontend
builder.AddProject<Projects.Shaspire_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WithReference(redis) // Often web apps need direct Redis access for sessions
    .WaitFor(apiService);

builder.Build().Run();