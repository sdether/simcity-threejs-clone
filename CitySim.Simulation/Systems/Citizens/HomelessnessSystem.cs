using Arch.Buffer;
using Arch.Core;
using CitySim.Simulation.Components;
using CitySim.Simulation.Entities;
using TypedArch;
using World = CitySim.Simulation.Model.World;

namespace CitySim.Simulation.Systems.Citizens;

public class HomelessnessSystem : SimSystem
{
    private static readonly TypedQueryDescription<ICitizen> CitizenQuery = TypedQueryDescription
        .Satisfies<ICitizen>()
        .WithAll<CitizenInfo>()
        .WithNone<Residency>();

    public override void Run(World world)
    {
        using var cmdBuffer = new CommandBuffer();
        world.Ecs.Query(in CitizenQuery.Inner,
            (Entity entity, ref CitizenInfo citizen) =>
            {
                // Citizen lost their home — sever workplace link and remove citizen from world.
                if (world.Ecs.Has<Job>(entity))
                {
                    cmdBuffer.Remove<Job>(entity);
                }
                cmdBuffer.Destroy(entity);
            }
        );
        cmdBuffer.Playback(world.Ecs);
    }
}