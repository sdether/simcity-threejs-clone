using Arch.Core;
using CitySim.Simulation.Components;
using World = CitySim.Simulation.Model.World;

namespace CitySim.Simulation.Systems.Buildings;

public class BuildingStateSystem : SimSystem
{
    public BuildingStateSystem() : base(new QueryDescription().WithAll<GridPosition, BuildingState, RoadAccessUser>())
    {
    }

    public override void Run(World world)
    {
        world.Ecs.Query(in QueryDescription,
            (Entity entity, ref GridPosition position,ref BuildingState buildingState, ref RoadAccessUser roadAccessUser) =>
            {
                var hasPower = true;
                if (world.Ecs.Has<PowerConsumer>(entity))
                {
                    var powerConsumer = world.Ecs.Get<PowerConsumer>(entity);
                    hasPower = powerConsumer.Supplied >= powerConsumer.Required;
                }
                var newStatus = !hasPower
                    ? BuildingStatus.NoPower
                    : !roadAccessUser.HasRoadAccess
                        ? BuildingStatus.NoRoadAccess
                        : BuildingStatus.Ok;
                if (buildingState.Status != newStatus)
                {
                    buildingState.Status = newStatus;
                    world.TileChanged(position);
                }
            }
        ); 
    }
}