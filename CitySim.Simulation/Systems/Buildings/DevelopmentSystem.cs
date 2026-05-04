using Arch.Core;
using CitySim.Simulation.Components;
using CitySim.Simulation.Entities;
using CitySim.Simulation.Extensions;
using TypedArch;
using Development = CitySim.Simulation.Components.Development;
using PowerConsumer = CitySim.Simulation.Components.PowerConsumer;

namespace CitySim.Simulation.Systems.Buildings;

using CitySim.Simulation.Model;

public class DevelopmentSystem : SimSystem
{
    private static readonly TypedQueryDescription<IBuilding> DevelopmentQuery = TypedQueryDescription
        .For<IBuilding>();

    public override void Run(World world)
    {
        world.Ecs.Query(in DevelopmentQuery.Inner,
            (Entity entity, ref GridPosition position, ref Development development, ref BuildingState buildingState,
                ref PowerConsumer powerConsumer, ref RoadAccessUser roadAccessUser) =>
            {
                CheckAbandonmentCriteria(ref development, ref powerConsumer, ref roadAccessUser);

                switch (development.State)
                {
                    case DevelopmentState.Undeveloped:
                        if (MeetsDevelopmentCriteria(ref powerConsumer, ref roadAccessUser) &&
                            Random.Shared.NextDouble() < SimConfig.Modules.Development.RedevelopChance)
                        {
                            development.State = DevelopmentState.UnderConstruction;
                            development.ConstructionCounter = 0;
                        }

                        break;

                    case DevelopmentState.UnderConstruction:
                        if (++development.ConstructionCounter == SimConfig.Modules.Development.ConstructionTime)
                        {
                            development.State = DevelopmentState.Developed;
                            development.Level = 1;
                            development.ConstructionCounter = 0;
                        }

                        break;

                    case DevelopmentState.Developed:
                        if (development.AbandonmentCounter > SimConfig.Modules.Development.AbandonThreshold)
                        {
                            if (Random.Shared.NextDouble() < SimConfig.Modules.Development.AbandonChance)
                                development.State = DevelopmentState.Abandoned;
                        }
                        else if (development.Level < development.MaxLevel &&
                                 Random.Shared.NextDouble() < SimConfig.Modules.Development.LevelUpChance)
                        {
                            development.Level++;
                            world.TileChanged(position);
                        }

                        break;

                    case DevelopmentState.Abandoned:
                        if (development.AbandonmentCounter == 0 &&
                            Random.Shared.NextDouble() < SimConfig.Modules.Development.RedevelopChance)
                            development.State = DevelopmentState.Developed;
                        break;
                }
            }
        );
    }

    private static bool MeetsDevelopmentCriteria(ref PowerConsumer powerConsumer, ref RoadAccessUser roadAccessUser) =>
        roadAccessUser.HasRoadAccess &&
        powerConsumer.Supplied >= powerConsumer.Required;

    private static void CheckAbandonmentCriteria(
        ref Development development,
        ref PowerConsumer powerConsumer,
        ref RoadAccessUser roadAccessUser
    )
    {
        if (!MeetsDevelopmentCriteria(ref powerConsumer, ref roadAccessUser))
            development.AbandonmentCounter++;
        else
            development.AbandonmentCounter = 0;
    }
}