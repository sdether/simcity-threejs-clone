using CitySim.Simulation.Model.Buildings;
using CitySim.Simulation.Model.Buildings.Power;
using CitySim.Simulation.Model.Buildings.Transportation;
using CitySim.Simulation.Model.Buildings.Zones;
using CitySim.Simulation.Systems.Buildings.Modules;

namespace CitySim.Simulation.Systems.Buildings;

using CitySim.Simulation.Model;
using Systems.Buildings.Modules;

public class BuildingManager
{
    private readonly DevelopmentModule _development = new();
    private readonly JobsModule        _jobs        = new();
    private readonly ResidentsModule   _residents   = new();
    private readonly RoadAccessModule  _roadAccess  = new();
    private readonly CommerceModule    _commerce    = new();

    public static Building? Create(Tile tile, string type)
    {
        if (tile.Building is not null) return null;
        return type switch
        {
            BuildingType.Residential => new ResidentialZone(tile, type),
            BuildingType.Commercial  => new CommercialZone(tile, type),
            BuildingType.Industrial  => new IndustrialZone(tile, type),
            BuildingType.Road        => new Road(tile, type),
            BuildingType.PowerPlant  => new PowerPlant(tile, type),
            BuildingType.PowerLine   => new PowerLine(tile, type),
            _                        => null,
        };
    }

    public void Bulldoze(World world, Tile tile)
    {
        if (tile.Building is null) return;
        foreach (var m in Modules(tile.Building))
            m.Dispose(world, tile.Building);
        tile.Building = null;
        tile.Updated  = true;
    }

    public void Simulate(World world, Tile tile)
    {
        if (tile.Building is null) return;
        var b = tile.Building;

        foreach (var m in Modules(b))
            m.Simulate(world, b);

        var newStatus =
            b.Power is { } p && p.Supplied < p.Required ? BuildingStatus.NoPower      :
            !b.HasRoadAccess                             ? BuildingStatus.NoRoadAccess :
                                                          BuildingStatus.Ok;

        if (newStatus != b.Status)
        {
            b.Status  = newStatus;
            b.Updated = b.Tile.Updated = true;
        }
    }

    private IEnumerable<SimModule> Modules(Building b) => b.Type switch
    {
        BuildingType.Residential => [_roadAccess, _development, _residents],
        BuildingType.Commercial  => [_roadAccess, _development, _jobs, _commerce],
        BuildingType.Industrial  => [_roadAccess, _development, _jobs],
        BuildingType.PowerPlant  => [_roadAccess],
        _                        => [],
    };
}
