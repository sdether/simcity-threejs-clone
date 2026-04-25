namespace CitySim.Simulation.Model.Buildings;


public abstract class Building
{
    private static int _nextId;

    public int     Id              { get; } = Interlocked.Increment(ref _nextId);
    public Tile    Tile            { get; }
    public string  Type            { get; }
    public string? Name            { get; set; }
    public bool    HideTerrain     { get; set; }
    public string  Status          { get; set; } = BuildingStatus.Ok;
    public bool    NeedsRoadAccess { get; set; }
    public bool    HasRoadAccess   { get; set; }
    public PowerConsumer? Power    { get; set; }
    public bool    Updated         { get; set; } = true;

    protected Building(Tile tile, string type, PowerConsumer? power = null, bool needsRoadAccess = false)
    {
        Tile           = tile;
        Type           = type;
        Power          = power;
        NeedsRoadAccess = needsRoadAccess;
        tile.Building  = this;
        tile.Updated   = true;
    }
}
