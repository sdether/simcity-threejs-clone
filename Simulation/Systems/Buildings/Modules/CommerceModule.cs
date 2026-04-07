using Simulation.Model.Buildings;
using Simulation.Model.Buildings.Zones;

namespace Simulation.Systems.Buildings.Modules;

using Simulation.Model;

public class CommerceModule : SimModule
{
    public override void Simulate(World world, Building building)
    {
        if (building is not CommercialZone zone) return;
        if (zone.Development.Level < 1) return;

        int    capacity          = SimConfig.Modules.Commerce.Capacity * zone.Development.Level;
        double staffingFraction  = (zone.Workers.Count + 1.0) / Math.Max(zone.MaxWorkers, 1);
        zone.Commerce.Capacity   = (int)Math.Round(capacity * staffingFraction);
    }
}
