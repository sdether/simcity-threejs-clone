using Arch.Core;
using Arch.Core.Extensions;
using CitySim.Simulation.Components;

namespace CitySim.Simulation.Managers;

using Model;

public static class CitizenFactory
{
    private static readonly string[] First =
    [
        "Emma", "Olivia", "Ava", "Sophia", "Isabella",
        "Liam", "Noah", "William", "James", "Benjamin",
        "Elizabeth", "Margaret", "Alice", "Dorothy", "Eleanor",
        "John", "Robert", "Charles", "Henry",
        "Alex", "Taylor", "Jordan", "Casey", "Robin",
    ];

    private static readonly string[] Last =
    [
        "Smith", "Johnson", "Williams", "Jones", "Brown",
        "Davis", "Miller", "Wilson", "Moore", "Taylor",
        "Anderson", "Thomas", "Jackson", "White", "Harris",
        "Clark", "Lewis", "Walker", "Hall", "Young",
        "Lee", "King", "Wright", "Adams", "Green",
    ];

    private static string CreateName() =>
        $"{First[Random.Shared.Next(First.Length)]} {Last[Random.Shared.Next(Last.Length)]}";
    
    public static Entity Create(World world)
    {
        var age = 1 + Random.Shared.Next(100);
        var state = age < SimConfig.Citizen.MinWorkingAge
            ? CitizenState.School
            : age >= SimConfig.Citizen.RetirementAge
                ? CitizenState.Retired
                : CitizenState.Unemployed;
        var citizen = world.Ecs.Create();
        world.EntitiesById.Add(citizen.Id, citizen);
        citizen.Add(
            new CitizenInfo(CreateName(), age),
            new CitizenStatus(state)
        );
        return citizen;
    }

    public static void Move(World world, Entity building, Entity citizen)
    {
        var replace = false;
        if (citizen.Has<Residency>())
        {
            ref var residency = ref citizen.Get<Residency>();
            replace = true;
            world.ResidentsByResidence[residency.Building].Remove(citizen);
        }

        var newResidency = new Residency(building);
        if (replace)
        {
            citizen.Set(newResidency);
        }
        else
        {
            citizen.Add(newResidency);
        }

        world.ResidentsByResidence[building].Add(citizen);
    }
}