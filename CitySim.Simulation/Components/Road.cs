namespace CitySim.Simulation.Components;


public readonly record struct Road(string Style);

public record struct RoadAccessUser(bool HasRoadAccess);