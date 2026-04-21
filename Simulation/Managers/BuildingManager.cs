using Arch.Core;
using Arch.Core.Extensions;
using Simulation.Components;
using Simulation.Extensions;
using Simulation.Model.Buildings;
using Simulation.Systems.Buildings.Modules;

using PowerConsumer      = Simulation.Components.PowerConsumer;
using PowerPlantComponent = Simulation.Components.PowerPlant;
using PowerLineComponent  = Simulation.Components.PowerLine;
using RoadComponent       = Simulation.Components.Road;

namespace Simulation.Managers;

using Simulation.Model;

public enum BuildingStatus
{
    NoPower,
    NoRoadAccess,
    Ok,
}

public class BuildingManager
{
    private static readonly string[] ResidencePrefixes          = ["Emerald", "Ivory", "Crimson", "Opulent", "Celestial", "Enchanted", "Serene", "Whispering", "Stellar", "Tranquil"];
    private static readonly string[] ResidenceSuffixes          = ["Tower", "Residence", "Manor", "Court", "Plaza", "House", "Mansion", "Place", "Villa", "Gardens"];
    private static readonly string[] CommercialPrefixes         = ["Prime", "Elite", "Global", "Exquisite", "Vibrant", "Luxury", "Innovative", "Sleek", "Premium", "Dynamic"];
    private static readonly string[] CommercialSuffixes         = ["Commerce", "Trade", "Marketplace", "Ventures", "Enterprises", "Retail", "Group", "Emporium", "Boutique", "Mall"];
    private static readonly string[] CommercialBusinessSuffixes = ["LLC", "Inc.", "Co.", "Corp.", "Ltd."];
    private static readonly string[] IndustrialPrefixes         = ["Apex", "Vortex", "Elevate", "Zenith", "Nova", "Synapse", "Pulse", "Enigma", "Catalyst", "Axiom"];
    private static readonly string[] IndustrialSuffixes         = ["Dynamics", "Ventures", "Solutions", "Technologies", "Innovations", "Industries", "Enterprises", "Systems", "Mechanics", "Manufacturing"];
    private static readonly string[] IndustrialBusinessSuffixes = ["LLC", "Inc.", "Co.", "Corp.", "Ltd."];

    private static string Pick(string[] arr) => arr[Random.Shared.Next(arr.Length)];

    private readonly DevelopmentModule _development = new();
    private readonly JobsModule        _jobs        = new();
    private readonly ResidentsModule   _residents   = new();
    private readonly RoadAccessModule  _roadAccess  = new();
    private readonly CommerceModule    _commerce    = new();

    public static void Create(World world, Entity tile, string type)
    {
        if (tile.Has<OccupantRef>()) return;

        var pos     = tile.Get<GridPosition>();
        var state   = new BuildingState(BuildingStatus.NoPower, true, false);
        var dev     = new Development(0, 0, 0);

        Entity building;
        switch (type)
        {
            case BuildingType.Residential:
                building = world.Ecs.Create(ArchetypeId.Residential,
                    new Occupant(), pos,
                    new PowerConsumer(10), dev, state,
                    new Residence($"{Pick(ResidencePrefixes)} {Pick(ResidenceSuffixes)}", 0));
                world.ResidentsByResidence[building] = [];
                break;

            case BuildingType.Commercial:
                building = world.Ecs.Create(ArchetypeId.Commercial,
                    new Occupant(), pos,
                    new PowerConsumer(10), dev, state,
                    new Commercial($"{Pick(CommercialPrefixes)} {Pick(CommercialSuffixes)} {Pick(CommercialBusinessSuffixes)}", 0),
                    new Employer(SimConfig.Modules.Jobs.MaxWorkers));
                world.EmployeesByEmployer[building] = [];
                break;

            case BuildingType.Industrial:
                building = world.Ecs.Create(ArchetypeId.Industrial,
                    new Occupant(), pos,
                    new PowerConsumer(10), dev, state,
                    new Industrial($"{Pick(IndustrialPrefixes)} {Pick(IndustrialSuffixes)} {Pick(IndustrialBusinessSuffixes)}"),
                    new Employer(SimConfig.Modules.Jobs.MaxWorkers));
                world.EmployeesByEmployer[building] = [];
                break;

            case BuildingType.Road:
                building = world.Ecs.Create(ArchetypeId.Road,
                    new Occupant(), pos,
                    new RoadComponent("default"));
                break;

            case BuildingType.PowerPlant:
                building = world.Ecs.Create(ArchetypeId.PowerPlant,
                    new Occupant(), pos,
                    new PowerPlantComponent(100, 0));
                break;

            case BuildingType.PowerLine:
                building = world.Ecs.Create(ArchetypeId.PowerLine,
                    new Occupant(), pos,
                    new PowerLineComponent());
                break;

            default:
                return;
        }

        tile.Add(new OccupantRef(building));
        tile.SafeAdd<Updated>();
    }

    public void Bulldoze(World world, Entity tile)
    {
        if (!tile.Has<OccupantRef>()) return;
        ref var occupantRef = ref tile.Get<OccupantRef>();
        tile.Remove<OccupantRef>();
        world.Ecs.Destroy(occupantRef.entity);
        tile.SafeAdd<Updated>();
    }

    public void BuildingStateSystem(World world)
    {
        var query = new QueryDescription().WithAll<PowerConsumer, BuildingState>();
        world.Ecs.World.Query(in query,
            (Entity entity, ref PowerConsumer powerConsumer, ref BuildingState buildingState) =>
            {
                var newStatus = powerConsumer.Supplied < powerConsumer.Required
                    ? BuildingStatus.NoPower
                    : !buildingState.HasRoadAccess
                        ? BuildingStatus.NoRoadAccess
                        : BuildingStatus.Ok;
                if (buildingState.Status != newStatus)
                {
                    buildingState.Status = newStatus;
                    entity.SafeAdd<Updated>();
                }
            });
    }
}
