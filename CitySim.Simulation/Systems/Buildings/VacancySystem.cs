using Arch.Buffer;
using Arch.Core;
using CitySim.Simulation.Components;
using CitySim.Simulation.Entities;
using TypedArch;
using World = CitySim.Simulation.Model.World;

namespace CitySim.Simulation.Systems.Buildings;

public class VacancySystem : SimSystem
{
    private static readonly TypedQueryDescription<IResidential> VacancyQuery = TypedQueryDescription
        .Satisfies<IResidential>()
        .WithAll<GridPosition, Vacancies>();
    

    public override void Run(World world)
    {
        using var cmdBuffer = new CommandBuffer();
        world.Ecs.Query(in VacancyQuery.Inner,
            (Entity building, ref GridPosition position, ref Vacancies vacancies) =>
            {
                var vacancyTotal = vacancies.Count;
                for (var i = 0; i < vacancyTotal; i++)
                {
                    if (Random.Shared.NextDouble() < SimConfig.Modules.Residents.ResidentMoveInChance)
                    {
                        var resident = CitizenFactory.Create(world);
                        cmdBuffer.Add(resident, new Residency(building));
                        vacancies.Count -= 1;
                        world.ResidentsByResidence[building].Add(resident);
                        world.TileChanged(position);
                    }
                }

                if (vacancies.Count == 0)
                {
                    cmdBuffer.Remove<Vacancies>(building);
                }
            }
        );
        cmdBuffer.Playback(world.Ecs);
    }
}