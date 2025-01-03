using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Housemate.PantryDb;

public class PantryDbContext(DbContextOptions<PantryDbContext> options) : DbContext(options)
{

    private static readonly Func<PantryDbContext, int?, int?, int, IAsyncEnumerable<PantryItem>> GetPantryItemsAfterQuery =
        EF.CompileAsyncQuery((PantryDbContext context, int? pantryCategoryId, int? after, int pageSize) =>
           context.PantryItems.AsNoTracking()
               .Where(pi => pantryCategoryId == null || pi.PantryCategoryId == pantryCategoryId)
               .Where(pi => after == null || pi.Id >= after)
               .Include(pi => pi.PantryCategory)
               .Include(pi => pi.PantryLocation)
               .OrderBy(pi => pi.Id)
               .Take(pageSize + 1));

    private static readonly Func<PantryDbContext, int?, int, int, IAsyncEnumerable<PantryItem>> GetPantryItemsBeforeQuery =
        EF.CompileAsyncQuery((PantryDbContext context, int? pantryLocationId, int before, int pageSize) =>
           context.PantryItems.AsNoTracking()
               .Where(pi => pantryLocationId == null || pi.PantryLocationId == pantryLocationId)
               .Where(pi => pi.Id <= before)
               .Include(pi => pi.PantryCategory)
               .Include(pi => pi.PantryLocation)
               .OrderByDescending(pi => pi.Id)
               .Take(pageSize + 1)
               .OrderBy(pi => pi.Id)
               .AsQueryable());

    public Task<List<PantryItem>> GetPantryItemsCompiledAsync(int? pantryCategoryId, int? before, int? after, int pageSize)
    {
        // Using keyset pagination: https://learn.microsoft.com/ef/core/querying/pagination#keyset-pagination
        return ToListAsync(before is null
            // Paging forward
            ? GetPantryItemsAfterQuery(this, pantryCategoryId, after, pageSize)
            // Paging backward
            : GetPantryItemsBeforeQuery(this, pantryCategoryId, before.Value, pageSize));
    }

    public DbSet<PantryItem> PantryItems => Set<PantryItem>();

    public DbSet<PantryCategory> PantryCategories => Set<PantryCategory>();

    public DbSet<PantryLocation> PantryLocations => Set<PantryLocation>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        DefinePantryCategory(builder.Entity<PantryCategory>());
        DefinePantryLocation(builder.Entity<PantryLocation>());
        DefinePantryItem(builder.Entity<PantryItem>());
    }

    private static void DefinePantryLocation(EntityTypeBuilder<PantryLocation> builder)
    {
        builder.ToTable("PantryLocation");

        builder.HasKey(ci => ci.Id);

        builder.Property(ci => ci.Id)
            .UseHiLo("pantry_location_hilo")
            .IsRequired();

        builder.Property(cb => cb.Location)
            .IsRequired()
            .HasMaxLength(100);
    }

    private static void DefinePantryItem(EntityTypeBuilder<PantryItem> builder)
    {
        builder.ToTable("Pantry");

        builder.Property(ci => ci.Id)
                    .UseHiLo("pantry_hilo")
                    .IsRequired();

        builder.Property(ci => ci.Name)
            .IsRequired(true)
            .HasMaxLength(100);

        builder.Property(ci => ci.Quantity)
            .IsRequired(true)
            .HasPrecision(5,1);

        builder.Property(ci => ci.Size)
            .IsRequired(false)
            .HasPrecision(5, 1);

        builder.Property(ci => ci.Units)
            .IsRequired(false);

        builder.HasOne(ci => ci.PantryLocation)
            .WithMany()
            .HasForeignKey(ci => ci.PantryLocationId);

        builder.HasOne(ci => ci.PantryCategory)
            .WithMany()
            .HasForeignKey(ci => ci.PantryCategoryId);
    }

    private static void DefinePantryCategory(EntityTypeBuilder<PantryCategory> builder)
    {
        builder.ToTable("PantryCategory");
        builder.HasKey(ci => ci.Id);

        builder.Property(ci => ci.Id)
            .UseHiLo("pantry_category_hilo")
            .IsRequired();

        builder.Property(cb => cb.Category)
            .IsRequired()
            .HasMaxLength(256);
    }

    private static async Task<List<T>> ToListAsync<T>(IAsyncEnumerable<T> asyncEnumerable)
    {
        var results = new List<T>();
        await foreach (var value in asyncEnumerable)
        {
            results.Add(value);
        }

        return results;
    }
}

