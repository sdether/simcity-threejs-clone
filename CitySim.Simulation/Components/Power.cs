namespace CitySim.Simulation.Components;

public record struct PowerConsumer(int Required, int Supplied);

public readonly record struct PowerConductor;

public record struct PowerPlant(int Capacity, int Consumed);

public readonly record struct PowerLine;

