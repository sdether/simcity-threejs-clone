using Arch.Core;
using CitySim.Simulation.Components;
using CitySim.Simulation.Entities;
using TypedArch;
using World = CitySim.Simulation.Model.World;

namespace CitySim.Simulation.Systems.Buildings;

public class BuildingStateSystem : SimSystem
{
    private static readonly TypedQueryDescription<IGridOccupantWithRoadAccess> BuildingStateQuery = TypedQueryDescription
        .For<IGridOccupantWithRoadAccess>();
    
    public override void Run(World world)
    {
        world.Ecs.Query(in BuildingStateQuery.Inner,
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