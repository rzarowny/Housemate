using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Housemate.PantryDb;

namespace Housemate.PantryDbManager;

internal class PantryDbInitializer(IServiceProvider serviceProvider, ILogger<PantryDbInitializer> logger)
    : BackgroundService
{
    public const string ActivitySourceName = "Migrations";

    private readonly ActivitySource _activitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PantryDbContext>();

        using var activity = _activitySource.StartActivity("Initializing catalog database", ActivityKind.Client);
        await InitializeDatabaseAsync(dbContext, cancellationToken);
    }

    public async Task InitializeDatabaseAsync(PantryDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();

        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(dbContext.Database.MigrateAsync, cancellationToken);

        await SeedAsync(dbContext, cancellationToken);

        logger.LogInformation("Database initialization completed after {ElapsedMilliseconds}ms", sw.ElapsedMilliseconds);
    }

    private async Task SeedAsync(PantryDbContext dbContext, CancellationToken cancellationToken)
    {
        logger.LogInformation("Seeding database");

        static List<PantryCategory> GetPreconfiguredPantryCategories()
        {
            return [
                new() { Category = "Canned" },
                new() { Category = "Produce" },
                new() { Category = "Beverages" },
                new() { Category = "Self-Packaged" },
                new() { Category = "Frozen" },
                new() { Category = "Other" },
            ];
        }

        static List<PantryLocation> GetPreconfiguredPantryLocations()
        {
            return [
                new() { Location = "Fridge", IsFreezer = true, IsFridge = true },
                new() { Location = "Garage Freezer", IsFreezer = true, IsFridge = false },
                new() { Location = "Downstairs Freezer", IsFreezer = true, IsFridge = false },
                new() { Location = "Downstairs Rack", IsFreezer = false, IsFridge = false },
                new() { Location = "Upstairs", IsFreezer = false, IsFridge = false },
            ];
        }

        static List<PantryItem> GetPreconfiguredItems(DbSet<PantryCategory> pantryCategories, DbSet<PantryLocation> pantryLocations)
        {
            var canned = pantryCategories.First(c => c.Category == "Canned");
            var frozen = pantryCategories.First(c => c.Category == "Frozen");
            var other = pantryCategories.First(c => c.Category == "Other");

            var fridge = pantryLocations.First(l => l.Location == "Fridge");
            var upstairs = pantryLocations.First(l => l.Location == "Upstairs");
            var garageFreezer = pantryLocations.First(l => l.Location == "Garage Freezer");

            return [
                new() { PantryCategory = canned, PantryLocation = upstairs, Quantity = 5.1F, Name = "Kidney Beans" },
                new() { PantryCategory = canned, PantryLocation = fridge, Quantity = 101.2F, Name = "Pickles" },
                new() { PantryCategory = frozen, PantryLocation = garageFreezer, Quantity = 1.5F, Name = "Burger Buns" },
                new() { PantryCategory = other, PantryLocation = garageFreezer, Quantity = 5.1F, Name = "Beefy bois" },
                new() { PantryCategory = other, PantryLocation = garageFreezer, Quantity = 69.69F, Name = "Shredded Cheese Packs", Size = 200, Units = "g" },
            ];
        }

        if (!dbContext.PantryCategories.Any())
        {
            var categories = GetPreconfiguredPantryCategories();
            await dbContext.PantryCategories.AddRangeAsync(categories, cancellationToken);

            logger.LogInformation("Seeding {PantryCategoryCount} pantry categories", categories.Count);

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (!dbContext.PantryLocations.Any())
        {
            var locations = GetPreconfiguredPantryLocations();
            await dbContext.PantryLocations.AddRangeAsync(locations, cancellationToken);

            logger.LogInformation("Seeding {PantryLocationCount} pantry locations", locations.Count);

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (!dbContext.PantryItems.Any())
        {
            var items = GetPreconfiguredItems(dbContext.PantryCategories, dbContext.PantryLocations);
            await dbContext.PantryItems.AddRangeAsync(items, cancellationToken);

            logger.LogInformation("Seeding {CatalogItemCount} catalog items", items.Count);

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
