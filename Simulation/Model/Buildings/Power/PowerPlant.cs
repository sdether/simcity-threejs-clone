namespace Simulation.Model.Buildings.Power;

public class PowerPlant : Building
{
    public int PowerCapacity { get; set; } = 100;
    public int PowerConsumed { get; set; }

    public int PowerAvailable => HasRoadAccess ? PowerCapacity - PowerConsumed : 0;

    public PowerPlant(Tile tile, string type) : base(tile, type, null, needsRoadAccess: true) { }
}
