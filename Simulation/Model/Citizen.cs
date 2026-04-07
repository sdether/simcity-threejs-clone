using Simulation.Model.Buildings.Zones;

namespace Simulation.Model;

public static class CitizenState
{
    public const string Idle       = "idle";
    public const string School     = "school";
    public const string Employed   = "employed";
    public const string Unemployed = "unemployed";
    public const string Retired    = "retired";
}

public class Citizen
{
    private static int _nextId;

    public int           Id           { get; } = Interlocked.Increment(ref _nextId);
    public string        Name         { get; }
    public string        State        { get; set; }
    public int           StateCounter { get; set; }
    public int           Age          { get; set; }
    public ResidentialZone? Residence { get; set; }
    public ZoneWithJobs?    Workplace { get; set; }
    public bool          Updated      { get; set; } = true;

    public Citizen()
    {
        Name = NameGenerator.Next();
        Age  = 1 + Random.Shared.Next(100);

        State = Age < SimConfig.Citizen.MinWorkingAge  ? CitizenState.School
              : Age >= SimConfig.Citizen.RetirementAge ? CitizenState.Retired
              :                                          CitizenState.Unemployed;
    }
}

internal static class NameGenerator
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

    public static string Next() =>
        $"{First[Random.Shared.Next(First.Length)]} {Last[Random.Shared.Next(Last.Length)]}";
}
