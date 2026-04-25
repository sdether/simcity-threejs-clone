using System.Text.Json.Serialization;

namespace CitySim.Simulation.Components;

public enum DevelopmentState
{
    Undeveloped,
    [JsonStringEnumMemberName("under-construction")]
    UnderConstruction,
    Developed,
    Abandoned,
}

public record struct Development(
    int Level,
    int AbandonmentCounter,
    int ConstructionCounter,
    DevelopmentState State,
    int MaxLevel)
{
    public static Development Create(
        int level               = 0,
        int abandonmentCounter  = 0,
        int constructionCounter = 0,
        DevelopmentState state  = DevelopmentState.Undeveloped,
        int maxLevel            = 3)
        => new(level, abandonmentCounter, constructionCounter, state, maxLevel);
}


public record struct Residence(string Name, int MaxResidents);

public record struct Vacancies(int Count);

public record struct Employer(int MaxWorkers);

public record struct Commercial(int Capacity);

public readonly record struct Industrial;
