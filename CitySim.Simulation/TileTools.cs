using Arch.Core;
using CitySim.Simulation.Components;

namespace Simulation;

using CitySim.Simulation.Model;

public static class TileTools
{

    /// <summary>
    /// BFS from (x, y) returning the first tile where <paramref name="filter"/> is true,
    /// or null if none is found within <paramref name="maxDistance"/> (Manhattan).
    /// </summary>
    public static Entity? FindTile(World world, Entity start, Func<Entity, bool> filter, int maxDistance)
    {
        var position = world.Ecs.Get<GridPosition>(start);
        
        var visited = new HashSet<GridPosition>();
        var queue   = new Queue<(Entity?,GridPosition)>();
        queue.Enqueue((start,position));

        while (queue.Count > 0)
        {
            var tile = queue.Dequeue();
            if (!visited.Add(tile.Item2)) continue;

            var dist = Math.Abs(position.X - tile.Item2.X) + Math.Abs(position.Y - tile.Item2.Y);
            if (dist > maxDistance) continue;

            foreach (var p in Neighbors(world, tile.Item2))
            {
                var e = world.GetTile(p);
                queue.Enqueue((e, p));
            }

            if (tile.Item1 is { } entity && filter(entity)) return entity;
        }
        return null;
    }

    private static IEnumerable<GridPosition> Neighbors(World world, GridPosition position)
    {
        if (position.X > 0)              yield return position with { X = position.X - 1 };
        if (position.X < world.Size - 1) yield return position with { X = position.X + 1 };
        if (position.Y > 0)              yield return position with { Y = position.Y - 1 };
        if (position.Y < world.Size - 1) yield return position with { Y = position.Y + 1 };
    }
}

public class NeighborMatches
{
    public bool Top    { get; init; }
    public bool Bottom { get; init; }
    public bool Left   { get; init; }
    public bool Right  { get; init; }
}
