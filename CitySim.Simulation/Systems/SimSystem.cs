using Arch.Core;
using TypedArch;
using World = CitySim.Simulation.Model.World;

namespace CitySim.Simulation.Systems;

public abstract class SimSystem : ISystem
{
    public abstract void Run(World world);
}
