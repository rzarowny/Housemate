var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .WithLifetime(ContainerLifetime.Persistent);

if (builder.ExecutionContext.IsRunMode)
{
    // Data volumes don't work on ACA for Postgres so only add when running
    postgres.WithDataVolume();
}

var pantryDb = postgres.AddDatabase("pantrydb");

var cache = builder.AddRedis("cache")
    .WithDataVolume()
    .WithRedisCommander();

var pantryDbManager = builder.AddProject<Projects.Housemate_PantryDbManager>("pantrydbmanager")
    .WithReference(pantryDb)
    .WaitFor(pantryDb)
    .WithHttpHealthCheck("/health")
    .WithHttpsCommand("/reset-db", "Reset Database", iconName: "DatabaseLightning");

var pantryService = builder.AddProject<Projects.Housemate_PantryService>("pantryservice")
    .WithReference(pantryDb)
    .WaitFor(pantryDbManager);

builder.AddProject<Projects.Housemate_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(pantryService)
    .WaitFor(pantryService);

builder.Build().Run();
