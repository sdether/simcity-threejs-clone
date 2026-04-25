using Arch.Core;
using CitySim.Simulation.Components;

namespace Simulation;

using CitySim.Simulation.Model;

// ── Event type constants (wire-protocol strings) ─────────────────────────────

public static class EventType
{
    public const string WorldSnapshot = "WorldSnapshot";
    public const string TileChanged = "TileChanged";
    public const string StatsChanged = "StatsChanged";
}

// ── Event records ─────────────────────────────────────────────────────────────

public abstract record SimEvent(string Type);

public record WorldSnapshotEvent(
    string Name,
    int Size,
    StatsSnapshot Stats,
    IReadOnlyList<TileSnapshot> Tiles
) : SimEvent(EventType.WorldSnapshot);

public record TileChangedEvent(TileSnapshot Tile) : SimEvent(EventType.TileChanged);

public record StatsChangedEvent(StatsSnapshot Stats) : SimEvent(EventType.StatsChanged);

// ── Snapshot value objects ────────────────────────────────────────────────────

public record DemandSnapshot(double Residential, double Commercial, double Industrial);

public record StatsSnapshot(int SimTime, int Population, DemandSnapshot Demand);

public record TileSnapshot(int X, int Y, string Terrain, BuildingSnapshot? Building);

public record DevelopmentSnapshot(DevelopmentState State, int Level);

public record PowerSnapshot(int Supplied, int Required);

public record CitizenSnapshot(string Name, int Age, CitizenState State);

public record WorkerSnapshot(int? MaxWorkers, IReadOnlyList<CitizenSnapshot>? Workers);

public record CommerceSnapshot(int Capacity);

public record BuildingSnapshot(
    BuildingType Type,
    BuildingStatus Status,
    bool HideTerrain,
    bool NeedsRoadAccess,
    bool HasRoadAccess,
    string? Name = null,
    DevelopmentSnapshot? Development = null,
    PowerSnapshot? Power = null,
    IReadOnlyList<CitizenSnapshot>? Residents = null,
    int? MaxResidents = null,
    IReadOnlyList<CitizenSnapshot>? Workers = null,
    int? MaxWorkers = null,
    CommerceSnapshot? Commerce = null
);

// ── Factory ───────────────────────────────────────────────────────────────────

public static class WorldEventFactory
{
    public static WorldSnapshotEvent WorldSnapshot(World world)
    {
        var tiles = new List<TileSnapshot>(world.Size * world.Size);
        for (int x = 0; x < world.Size; x++)
        for (int y = 0; y < world.Size; y++)
            tiles.Add(TileSnap(world, new GridPosition(x, y)));

        return new WorldSnapshotEvent(world.Name, world.Size, StatsSnap(world), tiles);
    }

    public static TileChangedEvent TileChanged(World world, GridPosition tile) => new(TileSnap(world, tile));

    public static StatsChangedEvent StatsChanged(World world) => new(StatsSnap(world));

    // ── private helpers ───────────────────────────────────────────────────────

    private static StatsSnapshot StatsSnap(World world) =>
        new(world.SimTime,
            world.ResidentsByResidence.Sum(rs => rs.Value.Count),
            DemandSnap(world.Stats.Demand)
        );

    private static TileSnapshot TileSnap(World world, GridPosition tile)
    {
        BuildingSnapshot? buildingSnapshot = null;
        var entity = world.GetTile(tile);
        if (entity is { } building)
        {
            buildingSnapshot = BuildingSnap(world, building);
        }
        return new(tile.X, tile.Y, "grass", buildingSnapshot);
    }

    private static DemandSnapshot DemandSnap(Demand d) =>
        new(d.Residential, d.Commercial, d.Industrial);

    private static BuildingSnapshot BuildingSnap(World world, Entity entity)
    {
        var b = world.Ecs.Get<BuildingState>(entity);

        DevelopmentSnapshot? developmentSnap = null;
        if (world.Ecs.Has<Development>(entity))
        {
            var development = world.Ecs.Get<Development>(entity);
            developmentSnap = new DevelopmentSnapshot(development.State, development.Level);
        }

        PowerSnapshot? powerSnap = null;
        if (world.Ecs.Has<PowerConsumer>(entity))
        {
            var powerConsumer = world.Ecs.Get<PowerConsumer>(entity);
            powerSnap = new PowerSnapshot(powerConsumer.Supplied, powerConsumer.Required);
        }

        var needsRoadAccess = false;
        var hasRoadAccess = false;
        if (world.Ecs.Has<RoadAccessUser>(entity))
        {
            needsRoadAccess = true;
            hasRoadAccess = world.Ecs.Get<RoadAccessUser>(entity).HasRoadAccess;
        }

        IReadOnlyList<CitizenSnapshot>? residents = null;
        int? maxResidents = null;
        var workerSnap = new WorkerSnapshot(null,null);
        CommerceSnapshot? commerceSnap = null;
        var hideTerrain = false;
        switch (b.Type)
        {
            case BuildingType.Residential:

                var residence = world.Ecs.Get<Residence>(entity);
                maxResidents = residence.MaxResidents;
                residents = world.ResidentsByResidence[entity]
                    .Select(citizen => CitizenSnap(world, citizen))
                    .ToList();
                break;
            case BuildingType.Commercial:
                var commercial = world.Ecs.Get<Commercial>(entity);
                commerceSnap = new CommerceSnapshot(commercial.Capacity);
                workerSnap = WorkerSnap(world, entity);
                break;
            case BuildingType.Industrial:
                workerSnap = WorkerSnap(world, entity);
                break;
            case BuildingType.Road:
                hideTerrain = true;
                break;
        }

        return new BuildingSnapshot(
            b.Type, b.Status, hideTerrain, needsRoadAccess, hasRoadAccess,
            b.Name, developmentSnap, powerSnap, residents, maxResidents,
            workerSnap.Workers, workerSnap.MaxWorkers, commerceSnap);
    }

    private static WorkerSnapshot WorkerSnap(World world, Entity entity)
    {
        var employer = world.Ecs.Get<Employer>(entity);
        return new WorkerSnapshot(
            employer.MaxWorkers,
            world.EmployeesByEmployer[entity]
                .Select(citizen => CitizenSnap(world, citizen))
                .ToList()
        );
    }

    private static CitizenSnapshot CitizenSnap(World world, Entity entity)
    {
        var citizenInfo = world.Ecs.Get<CitizenInfo>(entity);
        var citizenStatus = world.Ecs.Get<CitizenStatus>(entity);
        return new CitizenSnapshot(citizenInfo.Name, citizenInfo.Age, citizenStatus.State);
    }
}