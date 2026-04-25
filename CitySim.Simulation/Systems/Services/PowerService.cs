using CitySim.Simulation.Model.Buildings;
using CitySim.Simulation.Model.Buildings.Power;

namespace CitySim.Simulation.Systems.Services;

using CitySim.Simulation.Model;

/// <summary>
/// Distributes power from all power plants via BFS, sharing load evenly.
/// Frontiers for each plant are expanded simultaneously so buildings close
/// to multiple plants share the load rather than one plant serving all.
/// </summary>
public class PowerService : SimService
{
    public override void Simulate(World world)
    {
        // Collect power plants and power-consuming buildings.
        var plants = new List<PlantState>();
        var consumers = new Dictionary<(int, int), (Building Building, PowerConsumer Power)>();

        for (var x = 0; x < world.Size; x++)
        {
            for (var y = 0; y < world.Size; y++)
            {
                var tile = world.Tiles[x][y];
                if (tile.Building is PowerPlant pp)
                {
                    pp.PowerConsumed = 0;
                    var frontier = new Queue<Tile>();
                    frontier.Enqueue(tile);
                    plants.Add(new PlantState(pp, frontier, []));
                }
                else if (tile.Building?.Power is not null)
                {
                    consumers[(x, y)] = (tile.Building, tile.Building.Power.Copy());
                }
            }
        }

        if (plants.Count == 0) return;

        // Interleaved BFS — one step per plant per outer loop iteration.
        var searching = true;
        while (searching)
        {
            searching = false;
            foreach (var s in plants)
            {
                if (s.Plant.PowerAvailable == 0 || s.Frontier.Count == 0) continue;
                searching = true;

                var tile = s.Frontier.Dequeue();
                if (!s.Visited.Add(tile)) continue;

                if (consumers.TryGetValue((tile.X, tile.Y), out var entry))
                {
                    var (_, power) = entry;
                    if (power.Supplied < power.Required)
                    {
                        int supplied = Math.Min(s.Plant.PowerAvailable, power.Required);
                        s.Plant.PowerConsumed += supplied;
                        power.Supplied = supplied;
                    }
                }

                foreach (var neighbor in Neighbors(world, tile))
                {
                    if (!s.Visited.Contains(neighbor) && neighbor.Building is not null)
                        s.Frontier.Enqueue(neighbor);
                }
            }
        }

        // Apply computed power back to buildings, marking updated if changed.
        foreach (var (building, power) in consumers.Values)
        {
            if (!building.Power!.IsEqual(power))
            {
                building.Power   = power;
                building.Updated = building.Tile.Updated = true;
            }
        }
    }

    private static IEnumerable<Tile> Neighbors(World world, Tile tile)
    {
        int x = tile.X, y = tile.Y;
        if (x > 0)              yield return world.Tiles[x - 1][y];
        if (x < world.Size - 1) yield return world.Tiles[x + 1][y];
        if (y > 0)              yield return world.Tiles[x][y - 1];
        if (y < world.Size - 1) yield return world.Tiles[x][y + 1];
    }

    private sealed class PlantState(PowerPlant plant, Queue<Tile> frontier, HashSet<Tile> visited)
    {
        public PowerPlant   Plant    { get; } = plant;
        public Queue<Tile>  Frontier { get; } = frontier;
        public HashSet<Tile> Visited { get; } = visited;
    }
}
