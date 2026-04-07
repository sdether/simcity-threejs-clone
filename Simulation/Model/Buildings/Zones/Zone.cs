namespace Simulation.Model.Buildings.Zones;

public static class DevelopmentState
{
    public const string Abandoned        = "abandoned";
    public const string Developed        = "developed";
    public const string UnderConstruction = "under-construction";
    public const string Undeveloped      = "undeveloped";
}

public class Development
{
    public string State                { get; set; } = DevelopmentState.Undeveloped;
    public int    Level                { get; set; }
    public int    AbandonmentCounter   { get; set; }
    public int    ConstructionCounter  { get; set; }
    public int    MaxLevel             { get; set; } = 3;
}

public class Zone : Building
{
    public Development Development { get; } = new();

    public Zone(Tile tile, string type)
        : base(tile, type, new PowerConsumer(10), needsRoadAccess: true) { }
}
