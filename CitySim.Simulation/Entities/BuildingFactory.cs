using Arch.Core;
using Arch.Core.Extensions;
using CitySim.Simulation.Components;
using World = CitySim.Simulation.Model.World;

namespace CitySim.Simulation.Entities;

public class BuildingFactory
{
    private static readonly string[] ResidencePrefixes =
    [
        "Emerald", "Ivory", "Crimson", "Opulent", "Celestial", "Enchanted", "Serene", "Whispering", "Stellar",
        "Tranquil"
    ];

    private static readonly string[] ResidenceSuffixes =
        ["Tower", "Residence", "Manor", "Court", "Plaza", "House", "Mansion", "Place", "Villa", "Gardens"];

    private static readonly string[] CommercialPrefixes =
        ["Prime", "Elite", "Global", "Exquisite", "Vibrant", "Luxury", "Innovative", "Sleek", "Premium", "Dynamic"];

    private static readonly string[] CommercialSuffixes =
    [
        "Commerce", "Trade", "Marketplace", "Ventures", "Enterprises", "Retail", "Group", "Emporium", "Boutique", "Mall"
    ];

    private static readonly string[] CommercialBusinessSuffixes = ["LLC", "Inc.", "Co.", "Corp.", "Ltd."];

    private static readonly string[] IndustrialPrefixes =
        ["Apex", "Vortex", "Elevate", "Zenith", "Nova", "Synapse", "Pulse", "Enigma", "Catalyst", "Axiom"];

    private static readonly string[] IndustrialSuffixes =
    [
        "Dynamics", "Ventures", "Solutions", "Technologies", "Innovations", "Industries", "Enterprises", "Systems",
        "Mechanics", "Manufacturing"
    ];

    private static readonly string[] IndustrialBusinessSuffixes = ["LLC", "Inc.", "Co.", "Corp.", "Ltd."];

    private static string Pick(string[] arr) => arr[Random.Shared.Next(arr.Length)];

    public static void Create(World world, GridPosition position, BuildingType type)
    {
        if (world.GetTile(position).HasValue) return;
        var building = world.Ecs.Create(position, new PowerConductor());
        var buildingState = BuildingStatus.Ok;
        var name = "";
        world.EntitiesById.Add(building.Id, building);
        switch (type)
        {
            case BuildingType.Residential:
            case BuildingType.Commercial:
            case BuildingType.Industrial:
                building.Add(
                    new PowerConsumer(10, 0),
                    Development.Create(),
                    new RoadAccessUser(false)
                );
                break;
        }

        switch (type)
        {
            case BuildingType.Residential:
                name = $"{Pick(ResidencePrefixes)} {Pick(ResidenceSuffixes)}";
                world.Ecs.Add<Residence>(building);
                world.ResidentsByResidence[building] = [];
                break;

            case BuildingType.Commercial:
                name = $"{Pick(CommercialPrefixes)} {Pick(CommercialSuffixes)} {Pick(CommercialBusinessSuffixes)}";
                world.Ecs.Add<Commercial, Employer>(building);
                world.EmployeesByEmployer[building] = [];
                break;

            case BuildingType.Industrial:
                name = $"{Pick(IndustrialPrefixes)} {Pick(IndustrialSuffixes)} {Pick(IndustrialBusinessSuffixes)}";
                world.Ecs.Add<Industrial, Employer>(building);
                world.EmployeesByEmployer[building] = [];
                break;

            case BuildingType.Road:
                building.Add<Road>();
                break;

            case BuildingType.PowerPlant:
                building.Add(
                    new PowerPlant(50, 0),
                    new RoadAccessUser(false)
                );
                break;

            case BuildingType.PowerLine:
                building.Add<PowerLine>();
                break;
        }

        building.Add(new BuildingState(name, type, buildingState));
        world.SetTile(position, building);
    }

    public void Bulldoze(World world, GridPosition position)
    {
        world.RemoveTile(position);
    }
}