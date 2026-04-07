namespace Simulation.Model.Buildings.Transportation;

public class Road : Building
{
    public string Style { get; set; } = "straight";

    public Road(Tile tile, string type) : base(tile, type)
    {
        Name = "Road";
    }
}
