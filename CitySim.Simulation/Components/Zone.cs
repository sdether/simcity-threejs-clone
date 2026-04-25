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
    public static Development Default => new (0, 0, 0, DevelopmentState.Undeveloped, 3);
}


public record struct Residence(string Name, int MaxResidents);

public record struct Vacancies(int Count);

public record struct Employer(int MaxWorkers);

public record struct Commercial(int Capacity);

public readonly record struct Industrial;
