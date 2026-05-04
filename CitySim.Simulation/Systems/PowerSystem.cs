using Arch.Buffer;
using Arch.Core;
using CitySim.Simulation.Entities;
using TypedArch;

namespace CitySim.Simulation.Systems;

using CitySim.Simulation.Components;
using CitySim.Simulation.Model;

/// <summary>
/// Distributes power from all power plants via BFS, sharing load evenly.
/// Frontiers for each plant are expanded simultaneously so buildings close
/// to multiple plants share the load rather than one plant serving all.
/// </summary>
public class PowerSystem : SimSystem
{
    private static readonly ILogger<PowerSystem> _logger = SimLog.For<PowerSystem>();

    private static readonly TypedQueryDescription<IPowerPlant> PowerPlantQuery = TypedQueryDescription
        .For<IPowerPlant>();

    private static readonly TypedQueryDescription<IBuilding> PowerConsumerQuery = TypedQueryDescription
        .For<IBuilding>();
    
    public override void Run(World world)
    {
        // Collect power plants and power-consuming buildings.
        var plants = new Dictionary<GridPosition, PlantState>();
        world.Ecs.Query(in PowerPlantQuery.Inner,
            (Entity entity, ref GridPosition position, ref PowerPlant powerPlant) =>
            {
                var frontier = new Queue<GridPosition>();
                frontier.Enqueue(position);
                plants.Add(position, new PlantState(entity, powerPlant.Capacity, frontier, []));
            }
        );

        if (plants.Count == 0) return;

        // Collect power plants and power-consuming buildings.
        var consumers = new Dictionary<GridPosition, (Entity Entity, PowerConsumer Power)>();
        world.Ecs.Query(in PowerConsumerQuery.Inner,
            (Entity entity, ref GridPosition position, ref PowerConsumer powerConsumer) =>
            {
                consumers.Add(position, (entity, powerConsumer));
            }
        );
        if (consumers.Count == 0) return;
        
        // Interleaved BFS — one step per plant per outer loop iteration.
        var searching = true;
        while (searching)
        {
            searching = false;
            foreach (var s in plants.Values)
            {
                if (s.Capacity == 0 || s.Frontier.Count == 0) continue;
                searching = true;

                var position = s.Frontier.Dequeue();
                if (!s.Visited.Add(position)) continue;

                if (consumers.TryGetValue(position, out var entry))
                {
                    var (entity, power) = entry;
                    if (power.Supplied < power.Required)
                    {
                        var supplied = Math.Min(s.Capacity, power.Required);
                        s.Consumed += supplied;
                        power.Supplied = supplied;
                        consumers[position] = (entity, power);
                    }
                }

                foreach (var neighbor in Neighbors(world, position))
                {
                    if (!s.Visited.Contains(neighbor))
                    {
                        var tile = world.GetTile(neighbor);
                        if (tile is { } occupant && world.Ecs.Has<PowerConductor>(occupant))
                        {
                            s.Frontier.Enqueue(neighbor);
                        }
                    }
                }
            }
        }

        using var cmdBuffer = new CommandBuffer();
        world.Ecs.Query(in PowerPlantQuery.Inner,
            (Entity entity, ref GridPosition position, ref PowerPlant powerPlant) =>
            {
                var state = plants[position];
                var newPower = new PowerPlant(state.Capacity, state.Consumed);
                if (powerPlant != newPower)
                {
                    cmdBuffer.Set(in entity, in newPower);
                    world.TileChanged(position);
                }
            }
        );
        world.Ecs.Query(in PowerConsumerQuery.Inner,
            (Entity entity, ref GridPosition position, ref PowerConsumer powerConsumer) =>
            {
                var (_, power) = consumers[position];
                if (power != powerConsumer)
                {
                    cmdBuffer.Set(in entity, in power);
                    world.TileChanged(position);
                }
            }
        );
        cmdBuffer.Playback(world.Ecs);
    }

    private static IEnumerable<GridPosition> Neighbors(World world, GridPosition tile)
    {
        int x = tile.X, y = tile.Y;
        if (x > 0) yield return new GridPosition(x - 1, y);
        if (x < world.Size - 1) yield return new GridPosition(x + 1, y);
        if (y > 0) yield return new GridPosition(x, y - 1);
        if (y < world.Size - 1) yield return new GridPosition(x, y + 1);
    }

    private sealed class PlantState(
        Entity entity,
        int capacity,
        Queue<GridPosition> frontier,
        HashSet<GridPosition> visited
    )
    {
        public Entity Entity { get; } = entity;
        public int Capacity { get; } = capacity;
        public int Consumed { get; set; } = 0;
        public Queue<GridPosition> Frontier { get; } = frontier;
        public HashSet<GridPosition> Visited { get; } = visited;
    }
}