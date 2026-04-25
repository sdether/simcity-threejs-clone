using CitySim.Simulation.Model.Buildings;

namespace CitySim.Simulation.Model;

public class Tile
{
    private static int _nextId;

    public int       Id       { get; } = Interlocked.Increment(ref _nextId);
    public int       X        { get; }
    public int       Y        { get; }
    public string    Terrain  { get; set; } = "grass";
    public Building? Building { get; set; }
    public bool      Updated  { get; set; } = true;

    public Tile(int x, int y)
    {
        X = x;
        Y = y;
    }
}
