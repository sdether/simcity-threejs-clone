namespace CitySim.Simulation.Model;

public class World
{
    public string        Name     { get; }
    public int           Size     { get; }
    public int           SimTime  { get; set; }
    public Tile[][]      Tiles    { get; }
    public Stats         Stats    { get; } = new();
    public List<Citizen> Citizens { get; set; } = [];

    public World(string name, int size)
    {
        Name  = name;
        Size  = size;
        Tiles = new Tile[size][];
        for (int x = 0; x < size; x++)
        {
            Tiles[x] = new Tile[size];
            for (int y = 0; y < size; y++)
                Tiles[x][y] = new Tile(x, y);
        }
    }
}

public class Stats
{
    public Demand Demand { get; } = new();
}

public class Demand
{
    public double Residential { get; set; }
    public double Commercial  { get; set; }
    public double Industrial  { get; set; }
}
