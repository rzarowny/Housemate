namespace Housemate.PantryDb;

public sealed class PantryLocation
{
    public int Id { get; set; }
    public required string Location { get; set; }
    public required bool IsFridge { get; set; }
    public required bool IsFreezer { get; set; }
}
