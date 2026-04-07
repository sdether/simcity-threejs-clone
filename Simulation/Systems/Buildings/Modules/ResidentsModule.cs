using Simulation.Model.Buildings;
using Simulation.Model.Buildings.Zones;

namespace Simulation.Systems.Buildings.Modules;

using Simulation.Model;

public class ResidentsModule : SimModule
{
    public override void Simulate(World world, Building building)
    {
        if (building is not ResidentialZone zone) return;

        switch (zone.Development.State)
        {
            case DevelopmentState.Abandoned when zone.Residents.Count > 0:
                EvictAll(zone);
                return;
           
            case DevelopmentState.Developed:
            {
                var max = (int)Math.Pow(SimConfig.Modules.Residents.MaxResidents, zone.Development.Level);
                if (max != zone.MaxResidents)
                {
                    zone.MaxResidents = max;
                    zone.Updated = zone.Tile.Updated = true;
                }

                break;
            }
        }
    }

    public override void Dispose(World world, Building building)
    {
        if (building is ResidentialZone zone) EvictAll(zone);
    }

    private static void EvictAll(ResidentialZone zone)
    {
        if (zone.Residents.Count == 0) return;
        foreach (var resident in zone.Residents)
            resident.Workplace = null;
        zone.Residents.Clear();
        zone.Updated = zone.Tile.Updated = true;
    }
}
