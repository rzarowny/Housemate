using Microsoft.EntityFrameworkCore;
using Housemate.PantryDb;
using Housemate.PantryDbManager;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<PantryDbContext>("pantrydb", null,
    optionsBuilder => optionsBuilder.UseNpgsql(npgsqlBuilder =>
        npgsqlBuilder.MigrationsAssembly(typeof(Program).Assembly.GetName().Name)));

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(PantryDbInitializer.ActivitySourceName));

builder.Services.AddSingleton<PantryDbInitializer>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<PantryDbInitializer>());
builder.Services.AddHealthChecks()
    .AddCheck<PantryDbInitializerHealthCheck>("DbInitializer", null);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapPost("/reset-db", async (PantryDbContext dbContext, PantryDbInitializer dbInitializer, CancellationToken cancellationToken) =>
    {
        // Delete and recreate the database. This is useful for development scenarios to reset the database to its initial state.
        await dbContext.Database.EnsureDeletedAsync(cancellationToken);
        await dbInitializer.InitializeDatabaseAsync(dbContext, cancellationToken);
    });
}

app.MapDefaultEndpoints();

await app.RunAsync();