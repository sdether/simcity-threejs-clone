using Simulation.Model.Buildings;
using Simulation.Model.Buildings.Zones;

namespace Simulation.Systems;

using Simulation.Model;

// ── Event type constants (wire-protocol strings) ─────────────────────────────

public static class EventType
{
    public const string WorldSnapshot = "WorldSnapshot";
    public const string TileChanged   = "TileChanged";
    public const string StatsChanged  = "StatsChanged";
}

// ── Event records ─────────────────────────────────────────────────────────────

public abstract record SimEvent(string Type);

public record WorldSnapshotEvent(
    string Name,
    int    Size,
    int    SimTime,
    int    Population,
    DemandSnapshot            Demand,
    IReadOnlyList<TileSnapshot> Tiles
) : SimEvent(EventType.WorldSnapshot);

public record TileChangedEvent(
    int             X,
    int             Y,
    string          Terrain,
    BuildingSnapshot? Building
) : SimEvent(EventType.TileChanged);

public record StatsChangedEvent(
    int            SimTime,
    int            Population,
    DemandSnapshot Demand
) : SimEvent(EventType.StatsChanged);

// ── Snapshot value objects ────────────────────────────────────────────────────

public record DemandSnapshot(double Residential, double Commercial, double Industrial);
public record TileSnapshot(int X, int Y, string Terrain, BuildingSnapshot? Building);
public record DevelopmentSnapshot(string State, int Level);
public record PowerSnapshot(int Supplied, int Required);
public record CitizenSnapshot(string Name, int Age, string State);
public record CommerceSnapshot(int Capacity);

public record BuildingSnapshot(
    string  Type,
    string  Status,
    bool    HideTerrain,
    bool    NeedsRoadAccess,
    bool    HasRoadAccess,
    string? Name          = null,
    DevelopmentSnapshot?            Development  = null,
    PowerSnapshot?                  Power        = null,
    IReadOnlyList<CitizenSnapshot>? Residents    = null,
    int?                            MaxResidents = null,
    IReadOnlyList<CitizenSnapshot>? Workers      = null,
    int?                            MaxWorkers   = null,
    CommerceSnapshot?               Commerce     = null
);

// ── Factory ───────────────────────────────────────────────────────────────────

public static class WorldEventFactory
{
    public static WorldSnapshotEvent WorldSnapshot(World world)
    {
        var tiles = new List<TileSnapshot>(world.Size * world.Size);
        for (int x = 0; x < world.Size; x++)
            for (int y = 0; y < world.Size; y++)
                tiles.Add(TileSnap(world.Tiles[x][y]));

        return new WorldSnapshotEvent(
            world.Name, world.Size, world.SimTime, world.Citizens.Count,
            DemandSnap(world.Stats.Demand), tiles);
    }

    public static TileChangedEvent TileChanged(Tile tile) =>
        new(tile.X, tile.Y, tile.Terrain,
            tile.Building is { } b ? BuildingSnap(b) : null);

    public static StatsChangedEvent StatsChanged(World world) =>
        new(world.SimTime, world.Citizens.Count, DemandSnap(world.Stats.Demand));

    // ── private helpers ───────────────────────────────────────────────────────

    private static TileSnapshot TileSnap(Tile tile) =>
        new(tile.X, tile.Y, tile.Terrain,
            tile.Building is { } b ? BuildingSnap(b) : null);

    private static DemandSnapshot DemandSnap(Demand d) =>
        new(d.Residential, d.Commercial, d.Industrial);

    private static BuildingSnapshot BuildingSnap(Building b)
    {
        var development = b is Zone z
            ? new DevelopmentSnapshot(z.Development.State, z.Development.Level)
            : null;

        var power = b.Power is { } p ? new PowerSnapshot(p.Supplied, p.Required) : null;

        IReadOnlyList<CitizenSnapshot>? residents = null;
        int? maxResidents = null;
        if (b is ResidentialZone rz)
        {
            residents    = rz.Residents.Select(r => new CitizenSnapshot(r.Name, r.Age, r.State)).ToList();
            maxResidents = rz.MaxResidents;
        }

        IReadOnlyList<CitizenSnapshot>? workers = null;
        int? maxWorkers = null;
        if (b is ZoneWithJobs zwj)
        {
            workers    = zwj.Workers.Select(w => new CitizenSnapshot(w.Name, w.Age, w.State)).ToList();
            maxWorkers = zwj.MaxWorkers;
        }

        var commerce = b is CommercialZone cz ? new CommerceSnapshot(cz.Commerce.Capacity) : null;

        return new BuildingSnapshot(
            b.Type, b.Status, b.HideTerrain, b.NeedsRoadAccess, b.HasRoadAccess,
            b.Name, development, power, residents, maxResidents, workers, maxWorkers, commerce);
    }
}
