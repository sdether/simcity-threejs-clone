namespace Simulation.Systems;

using Simulation.Model;

public static class TileTools
{
    public static Tile? GetTile(World world, int x, int y)
    {
        if (x < 0 || y < 0 || x >= world.Size || y >= world.Size) return null;
        return world.Tiles[x][y];
    }

    public static NeighborMatches GetMatchingNeighbors(World world, int x, int y, string type) => new()
    {
        Top    = GetTile(world, x, y - 1)?.Building?.Type == type,
        Bottom = GetTile(world, x, y + 1)?.Building?.Type == type,
        Left   = GetTile(world, x - 1, y)?.Building?.Type == type,
        Right  = GetTile(world, x + 1, y)?.Building?.Type == type,
    };

    /// <summary>
    /// BFS from (x, y) returning the first tile where <paramref name="filter"/> is true,
    /// or null if none is found within <paramref name="maxDistance"/> (Manhattan).
    /// </summary>
    public static Tile? FindTile(World world, int x, int y, Func<Tile, bool> filter, int maxDistance)
    {
        var start = GetTile(world, x, y);
        if (start is null) return null;

        var visited = new HashSet<int>();
        var queue   = new Queue<Tile>();
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var tile = queue.Dequeue();
            if (!visited.Add(tile.Id)) continue;

            int dist = Math.Abs(start.X - tile.X) + Math.Abs(start.Y - tile.Y);
            if (dist > maxDistance) continue;

            foreach (var n in Neighbors(world, tile.X, tile.Y))
                queue.Enqueue(n);

            if (filter(tile)) return tile;
        }
        return null;
    }

    private static IEnumerable<Tile> Neighbors(World world, int x, int y)
    {
        if (x > 0)              yield return world.Tiles[x - 1][y];
        if (x < world.Size - 1) yield return world.Tiles[x + 1][y];
        if (y > 0)              yield return world.Tiles[x][y - 1];
        if (y < world.Size - 1) yield return world.Tiles[x][y + 1];
    }
}

public class NeighborMatches
{
    public bool Top    { get; init; }
    public bool Bottom { get; init; }
    public bool Left   { get; init; }
    public bool Right  { get; init; }
}
