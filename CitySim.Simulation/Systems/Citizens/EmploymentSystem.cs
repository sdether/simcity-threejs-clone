using Arch.Buffer;
using Arch.Core;
using CitySim.Simulation.Components;
using CitySim.Simulation.Entities;
using Simulation;
using TypedArch;
using World = CitySim.Simulation.Model.World;

namespace CitySim.Simulation.Systems.Citizens;

public class EmploymentSystem : SimSystem
{
    private static readonly TypedQueryDescription<ICitizen> CitizenQuery = TypedQueryDescription
        .Satisfies<ICitizen>()
        .WithAll<CitizenInfo, CitizenStatus, Residency>()
        .WithNone<Job>();

    public override void Run(World world)
    {
        using var cmdBuffer = new CommandBuffer();
        world.Ecs.Query(in CitizenQuery.Inner,
            (Entity entity, ref CitizenInfo info, ref CitizenStatus status, ref Residency residency) =>
            {
                if(status.State != CitizenState.Unemployed) return;
                var employerCandidate = TileTools.FindTile(
                    world,
                    residency.Building,
                    t =>
                    {
                        if (!world.Ecs.Has<Employer>(t)) return false;
                        ref var employer = ref world.Ecs.Get<Employer>(t);
                        var employees = world.EmployeesByEmployer[t].Count;
                        return employer.MaxWorkers > employees;
                    },
                    SimConfig.Citizen.MaxJobSearchDistance);
                
                if (employerCandidate is { } employer)
                {
                    status.State = CitizenState.Employed;
                    world.Ecs.Add(entity, new Job(employer));
                    world.EmployeesByEmployer[employer].Add(entity);
                    world.TileChanged(world.Ecs.Get<GridPosition>(employer));
                }
               
            }
        );
    }


 
}