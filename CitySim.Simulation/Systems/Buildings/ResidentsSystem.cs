using Arch.Buffer;
using Arch.Core;
using Arch.Core.Extensions;
using CitySim.Simulation.Components;
using CitySim.Simulation.Extensions;

namespace CitySim.Simulation.Systems.Buildings;

using CitySim.Simulation.Model;

public class ResidentsSystem : SimSystem
{
    public ResidentsSystem() : base(new QueryDescription().WithAll<GridPosition, Residence, Development>())
    {
    }

    public override void Run(World world)
    {
        using var cmdBuffer = new CommandBuffer();
        world.Ecs.Query(in QueryDescription,
            (Entity entity, ref GridPosition position, ref Residence residence, ref Development development) =>
            {
                switch (development.State)
                {
                    case DevelopmentState.Abandoned when world.ResidentsByResidence.GetValueOrDefault(entity, []).Count > 0:
                        if (world.ResidentsByResidence.TryGetValue(entity, out var residents))
                        {
                            foreach (var resident in residents)
                            {
                                cmdBuffer.Remove<Residency>(resident);
                            }

                            world.ResidentsByResidence.Remove(entity);
                            world.TileChanged(position);
                        }
                        break;

                    case DevelopmentState.Developed:
                    {
                        var max = (int)Math.Pow(SimConfig.Modules.Residents.MaxResidents, development.Level);
                        if (max != residence.MaxResidents)
                        {
                            residence.MaxResidents = max;
                            if (world.Ecs.Has<Vacancies>(entity))
                            {
                                cmdBuffer.Remove<Vacancies>(entity);
                            }

                            var vacancies = residence.MaxResidents - world.ResidentsByResidence[entity].Count;
                            if (vacancies > 0 )
                            {
                                cmdBuffer.Add(entity, new Vacancies(vacancies));
                            }
                            world.TileChanged(position);
                        }

                        break;
                    }
                }
            }
        );
        cmdBuffer.Playback(world.Ecs);
    }
}