using System.Net;
using Arch.Core;
using Arch.Core.Extensions;
using CitySim.Simulation.Components;
using CitySim.Simulation.Extensions;
using Development = CitySim.Simulation.Components.Development;

namespace CitySim.Simulation.Systems.Buildings;

using CitySim.Simulation.Model;

public class JobsSystem : SimSystem
{
    public JobsSystem() : base(new QueryDescription().WithAll<GridPosition,Development,Employer>())
    {
    }

    public override void Run(World world)
    {
        var layoffs = new List<Entity>();
        world.Ecs.Query(in QueryDescription,
            (Entity entity, ref GridPosition position, ref Development development, ref Employer employer) =>
            {
                if (development.State == DevelopmentState.Abandoned)
                {
                    layoffs.Add(entity);
                    return;
                }

                var max = development.State == DevelopmentState.Developed
                    ? (int)Math.Pow(SimConfig.Modules.Jobs.MaxWorkers, development.Level)
                    : 0;

                if (max != employer.MaxWorkers)
                {
                    employer.MaxWorkers = max;
                    world.TileChanged(position);
                }
            }
        );
        
        foreach (var employer in layoffs)
        {
            if (world.EmployeesByEmployer.TryGetValue(employer, out var employees))
            {
                foreach (var employee in employees)
                {
                    employee.Remove<Job>();
                    employee.Set(new CitizenStatus(CitizenState.Unemployed));
                }

                world.EmployeesByEmployer.Remove(employer);
            }
        }
    }

}
