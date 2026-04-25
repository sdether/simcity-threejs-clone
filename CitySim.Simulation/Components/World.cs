namespace CitySim.Simulation.Components;

public readonly record struct WorldInfo(string Name, int Size, int SimTime);

public readonly record struct Demand(double Residential, double Commercial, double Industrial);
