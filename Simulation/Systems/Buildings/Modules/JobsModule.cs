using Simulation.Model.Buildings;
using Simulation.Model.Buildings.Zones;

namespace Simulation.Systems.Buildings.Modules;

using Simulation.Model;

public class JobsModule : SimModule
{
    public override void Simulate(World world, Building building)
    {
        if (building is not ZoneWithJobs zone) return;

        if (zone.Development.State == DevelopmentState.Abandoned)
        {
            LayOff(zone);
            return;
        }

        var max = zone.Development.State == DevelopmentState.Developed
            ? (int)Math.Pow(SimConfig.Modules.Jobs.MaxWorkers, zone.Development.Level)
            : 0;

        if (max != zone.MaxWorkers)
        {
            zone.MaxWorkers = max;
            zone.Updated = zone.Tile.Updated = true;
        }
    }

    public override void Dispose(World world, Building building)
    {
        if (building is ZoneWithJobs zone) LayOff(zone);
    }

    private static void LayOff(ZoneWithJobs zone)
    {
        foreach (var worker in zone.Workers)
            worker.Workplace = null;
        zone.Workers.Clear();
        zone.Updated = zone.Tile.Updated = true;
    }
}
