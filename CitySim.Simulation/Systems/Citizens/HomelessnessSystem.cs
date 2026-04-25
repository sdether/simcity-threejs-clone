using Arch.Buffer;
using Arch.Core;
using CitySim.Simulation.Components;
using World = CitySim.Simulation.Model.World;

namespace CitySim.Simulation.Systems.Citizens;

public class HomelessnessSystem : SimSystem
{
    public HomelessnessSystem() : base(new QueryDescription()
        .WithAll<CitizenInfo>()
        .WithNone<Residency>())
    {
    }

    public override void Run(World world)
    {
        using var cmdBuffer = new CommandBuffer();
        world.Ecs.Query(in QueryDescription,
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