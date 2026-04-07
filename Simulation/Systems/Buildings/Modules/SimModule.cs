using Simulation.Model.Buildings;

namespace Simulation.Systems.Buildings.Modules;

using Simulation.Model;

public abstract class SimModule
{
    public abstract void Simulate(World world, Building building);
    public virtual  void Dispose (World world, Building building) { }
}
