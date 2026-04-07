using Simulation.Model.Buildings;
using Simulation.Model.Buildings.Zones;

namespace Simulation.Systems;

using Model;

public static class CitizenManager
{
    /// <summary>
    /// Advances a citizen's state machine by one tick.
    /// Returns false if the citizen should be removed from the world (no residence).
    /// </summary>
    public static bool Simulate(World world, Citizen citizen)
    {
        if (citizen.Residence is null)
        {
            // Citizen lost their home — sever workplace link and remove from world.
            if (citizen.Workplace is { } workplace)
            {
                workplace.Workers.Remove(citizen);
                workplace.Updated = workplace.Tile.Updated = true;
                citizen.Workplace = null;
            }
            return false;
        }

        switch (citizen.State)
        {
            case CitizenState.Idle:
            case CitizenState.School:
            case CitizenState.Retired:
                break;

            case CitizenState.Unemployed:
                FindJob(world, citizen);
                if (citizen.Workplace is not null)
                {
                    citizen.State = CitizenState.Employed;
                    citizen.Residence.Updated = citizen.Residence.Tile.Updated = true;
                }
                break;

            case CitizenState.Employed:
                if (citizen.Workplace is null)
                {
                    citizen.State = CitizenState.Unemployed;
                    citizen.Residence.Updated = citizen.Residence.Tile.Updated = true;
                }
                break;
        }
        return true;
    }

    private static void FindJob(World world, Citizen citizen)
    {
        var tile = TileTools.FindTile(
            world,
            citizen.Residence!.Tile.X,
            citizen.Residence.Tile.Y,
            t => t.Building is ZoneWithJobs
            {
                Type: BuildingType.Commercial or BuildingType.Industrial, 
                AvailableJobs: > 0
            },
            SimConfig.Citizen.MaxJobSearchDistance);

        if (tile?.Building is ZoneWithJobs workplace)
        {
            workplace.Workers.Add(citizen);
            citizen.Workplace    = workplace;
            workplace.Updated    = tile.Updated = true;
        }
    }
}
