using CitySim.Simulation.Model.Buildings;
using CitySim.Simulation.Model.Buildings.Zones;

namespace CitySim.Simulation.Systems.Buildings.Modules;

using CitySim.Simulation.Model;

public class DevelopmentModule : SimModule
{
    public override void Simulate(World world, Building building)
    {
        if (building is not Zone zone) return;

        CheckAbandonmentCriteria(zone);
        var dev = zone.Development;

        switch (dev.State)
        {
            case DevelopmentState.Undeveloped:
                if (MeetsDevelopmentCriteria(zone) &&
                    Random.Shared.NextDouble() < SimConfig.Modules.Development.RedevelopChance)
                {
                    dev.State = DevelopmentState.UnderConstruction;
                    dev.ConstructionCounter = 0;
                }
                break;

            case DevelopmentState.UnderConstruction:
                if (++dev.ConstructionCounter == SimConfig.Modules.Development.ConstructionTime)
                {
                    dev.State = DevelopmentState.Developed;
                    dev.Level = 1;
                    dev.ConstructionCounter = 0;
                }
                break;

            case DevelopmentState.Developed:
                if (dev.AbandonmentCounter > SimConfig.Modules.Development.AbandonThreshold)
                {
                    if (Random.Shared.NextDouble() < SimConfig.Modules.Development.AbandonChance)
                        dev.State = DevelopmentState.Abandoned;
                }
                else if (dev.Level < dev.MaxLevel &&
                         Random.Shared.NextDouble() < SimConfig.Modules.Development.LevelUpChance)
                {
                    dev.Level++;
                    zone.Updated = zone.Tile.Updated = true;
                }
                break;

            case DevelopmentState.Abandoned:
                if (dev.AbandonmentCounter == 0 &&
                    Random.Shared.NextDouble() < SimConfig.Modules.Development.RedevelopChance)
                    dev.State = DevelopmentState.Developed;
                break;
        }
    }

    private static bool MeetsDevelopmentCriteria(Zone zone) =>
        zone.HasRoadAccess &&
        zone.Power is { } p && p.Supplied >= p.Required;

    private static void CheckAbandonmentCriteria(Zone zone)
    {
        if (!MeetsDevelopmentCriteria(zone))
            zone.Development.AbandonmentCounter++;
        else
            zone.Development.AbandonmentCounter = 0;
    }
}
