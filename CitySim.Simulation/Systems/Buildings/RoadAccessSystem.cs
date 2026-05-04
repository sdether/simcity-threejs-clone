using Arch.Core;
using CitySim.Simulation.Components;
using CitySim.Simulation.Entities;
using Simulation;
using TypedArch;

namespace CitySim.Simulation.Systems.Buildings;

using CitySim.Simulation.Model;

public class RoadAccessSystem : SimSystem
{
    private static readonly TypedQueryDescription<IGridOccupantWithRoadAccess> RoadQuery = TypedQueryDescription
        .Satisfies<IGridOccupantWithRoadAccess>()
        .WithAll<GridPosition, RoadAccessUser>();

    public override void Run(World world)
    {
        world.Ecs.Query(in RoadQuery.Inner,
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