using System.Globalization;

namespace Housemate.Web.Services;

public class PantryServiceClient(HttpClient client)
{
    public Task<PantryItemsPage?> GetItemsAsync(int? before = null, int? after = null)
    {
        // Make the query string with encoded parameters
        var query = (before, after) switch
        {
            (null, null) => default,
            (int b, null) => QueryString.Create("before", b.ToString(CultureInfo.InvariantCulture)),
            (null, int a) => QueryString.Create("after", a.ToString(CultureInfo.InvariantCulture)),
            _ => throw new InvalidOperationException(),
        };

        return client.GetFromJsonAsync<PantryItemsPage>($"api/v1/pantry/items/location/all{query}");
    }
}

public record PantryItemsPage(int FirstId, int NextId, bool IsLastPage, IEnumerable<PantryItem> Data);

public record PantryItem
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public required float Quantity { get; init; }
    public float? Size { get; init; }
    public string? Units { get; init; }
    public int PantryLocationId { get; init; }
    public required PantryLocation PantryLocation { get; init; }
    public int PantryCategoryId { get; init; }
    public required PantryCategory PantryCategory { get; init; }
}

public record PantryLocation
{
    public int Id { get; init; }
    public required string Location {  get; init; }
}

public record PantryCategory
{
    public int Id { get; init; }
    public required string Category { get; init; }
}