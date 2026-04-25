using System.Text.Json.Serialization;

namespace CitySim.Simulation.Components;

public enum BuildingStatus
{
    [JsonStringEnumMemberName("no-power")]
    NoPower,
    [JsonStringEnumMemberName("no-road-access")]
    NoRoadAccess,
    Ok,
}

public enum BuildingType
{
    Residential,
    Commercial,
    Industrial,
    Road,
    [JsonStringEnumMemberName("power-plant")]
    PowerPlant,
    [JsonStringEnumMemberName("power-line")]
    PowerLine,
}

public record struct BuildingState(string Name, BuildingType Type, BuildingStatus Status);
