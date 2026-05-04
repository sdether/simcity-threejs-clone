using CitySim.Simulation.Components;
using TypedArch;

namespace CitySim.Simulation.Entities;

interface ICitizen : IArcheType
{
    CitizenInfo CitizenInfo { get; }
    CitizenStatus CitizenStatus { get; }
    Residency? Residency { get; }
    Job? Job { get; }
}