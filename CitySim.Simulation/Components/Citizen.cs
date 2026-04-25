using Arch.Core;

namespace CitySim.Simulation.Components;

public readonly record struct CitizenInfo(string Name, int Age);

public record struct CitizenStatus(CitizenState State);

public readonly record struct Residency(Entity Building);


public readonly record struct Job(Entity Building);

public enum CitizenState
{
    Idle,
    School,
    Employed,
    Unemployed,
    Retired,
}