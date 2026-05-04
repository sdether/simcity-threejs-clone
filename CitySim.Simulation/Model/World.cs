using Arch.Core;
using CitySim.Simulation.Components;
using CitySim.Simulation.Entities;
using CitySim.Simulation.Systems;
using CitySim.Simulation.Systems.Buildings;
using CitySim.Simulation.Systems.Citizens;
using TypedArch;

namespace CitySim.Simulation.Model;

using EcsWorld = Arch.Core.World;

interface IArcheTypes
{
    ICitizen Citizen { get; }
    IPowerLine PowerLine { get; }
    IRoad Road { get; }
    IPowerPlant PowerPlant { get; }
    IResidential Residential { get; }
    ICommercial Commercial { get; }
    IIndustrial Industrial { get; } 
}

interface ISystems 
{
    BuildingStateSystem BuildingStateSystem { get; }
    CommerceSystem CommerceSystem { get; }
    DevelopmentSystem DevelopmentSystem { get; }
    JobsSystem JobsSystem { get; }
    ResidentsSystem ResidentsSystem { get; }
    RoadAccessSystem RoadAccessSystem { get; }
    VacancySystem VacancySystem { get; }
    EmploymentSystem EmploymentSystem { get; }
    HomelessnessSystem HomelessnessSystem { get; }
    PowerSystem PowerSystem { get; }
}

public class World
{
    public string        Name     { get; }
    public int           Size     { get; }
    public int           SimTime  { get; set; }
    public Stats         Stats    { get; } = new();
    
    public readonly Dictionary<int, Entity> EntitiesById = new ();
    public readonly Dictionary<Entity, HashSet<Entity>> EmployeesByEmployer = new();
    public readonly Dictionary<Entity, HashSet<Entity>> ResidentsByResidence = new();

    private readonly TypedWorld<IArcheTypes, ISystems> _typedEcs = new();
    private readonly HashSet<GridPosition> _changedTiles = [];
    private readonly Dictionary<GridPosition,Entity?> _tiles =new();

    public EcsWorld Ecs => _typedEcs.Inner;
    
    public World(string name, int size)
    {
        Name  = name;
        Size  = size;
        for (var x = 0; x < size; x++)
        {
            for (var y = 0; y < size; y++)
            {
                var position = new GridPosition(x, y);
                _tiles[position] = null;
            }
        }
    }

    public void TileChanged(GridPosition position)
    {
        _changedTiles.Add(position);
    }

    public IEnumerable<GridPosition> ResetChangedTiles()
    {
        var changes = _changedTiles.ToArray();
        _changedTiles.Clear();
        return changes;
    }
    
    public GridPosition? GetGridPosition( int x, int y)
    {
        if (x < 0 || y < 0 || x >= Size || y >= Size) return null;
        return new GridPosition(x, y);
    }

    public Entity? GetTile(GridPosition gridPosition)
    {
        return _tiles[gridPosition];
    }

    
    public void SetTile(GridPosition position, Entity tile)
    {
        _tiles[position] = tile;
        TileChanged(position);
    }

    public void RemoveTile(GridPosition position)
    {
        if (GetTile(position) is { } tile)
        {
            EntitiesById.Remove(tile.Id);
            _tiles.Remove(position);
            Ecs.Destroy(tile);
            TileChanged(position);
        }
    }
}

public class Stats
{
    public Demand Demand { get; } = new();
}

public class Demand
{
    public double Residential { get; set; }
    public double Commercial  { get; set; }
    public double Industrial  { get; set; }
}
