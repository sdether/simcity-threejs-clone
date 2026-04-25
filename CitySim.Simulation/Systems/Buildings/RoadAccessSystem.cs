using Arch.Core;
using CitySim.Simulation.Components;
using Simulation;

namespace CitySim.Simulation.Systems.Buildings;

using CitySim.Simulation.Model;

public class RoadAccessSystem : SimSystem
{
    public RoadAccessSystem() : base(new QueryDescription().WithAll<GridPosition, RoadAccessUser>())
    {
    }

    public override void Run(World world)
    {
        world.Ecs.Query(in QueryDescription,
            (Entity entity, ref GridPosition position, ref RoadAccessUser roadAccessUser) =>
            {
                var road = TileTools.FindTile(
                    world,
                    entity,
                    t => world.Ecs.Has<Road>(t),
                    SimConfig.Modules.RoadAccess.SearchDistance);

                var has = road is not null;
                if (roadAccessUser.HasRoadAccess != has)
                {
                    roadAccessUser.HasRoadAccess = has;
                    world.TileChanged(position);
                }
            }
        );
    }
}