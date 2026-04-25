namespace CitySim.Simulation.Model.Buildings.Zones;

public class ZoneWithJobs : Zone
{
    public List<Citizen> Workers    { get; set; } = [];
    public int           MaxWorkers { get; set; }

    public int AvailableJobs => MaxWorkers - Workers.Count;

    public ZoneWithJobs(Tile tile, string type) : base(tile, type) { }
}
