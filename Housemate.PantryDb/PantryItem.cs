namespace Housemate.PantryDb;

public class PantryItem
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required float Quantity { get; set; }
    public float? Size { get; set; }
    public string? Units { get; set; }

    public int PantryLocationId { get; set; }
    public required virtual PantryLocation PantryLocation { get; set; }

    public int PantryCategoryId { get; set; }
    public required virtual PantryCategory PantryCategory { get; set; }

}
