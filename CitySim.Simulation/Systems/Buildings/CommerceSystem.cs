using Arch.Core;
using CitySim.Simulation.Components;

namespace CitySim.Simulation.Systems.Buildings;

using CitySim.Simulation.Model;

public class CommerceSystem : SimSystem
{
    public CommerceSystem() : base(new QueryDescription().WithAll<GridPosition,Development, Commercial, Employer>())
    {
    }


    public override void Run(World world)
    {
        world.Ecs.Query(in QueryDescription,
            (Entity entity, ref GridPosition position, ref Development development, ref Commercial commercial, ref Employer employer) =>
            {
                if (development.Level < 1) return;

                var capacity = SimConfig.Modules.Commerce.Capacity * development.Level;
                var employees = world.EmployeesByEmployer[entity].Count;
                var staffingFraction = (employees + 1.0) / Math.Max(employer.MaxWorkers, 1);
                var newCapacity = (int)Math.Round(capacity * staffingFraction);
                if (newCapacity != commercial.Capacity)
                {
                    commercial.Capacity = newCapacity;
                    world.TileChanged(position);
                }
            }
        );
    }
}