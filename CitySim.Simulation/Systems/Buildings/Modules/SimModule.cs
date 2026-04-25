using CitySim.Simulation.Model.Buildings;

namespace CitySim.Simulation.Systems.Buildings.Modules;

using CitySim.Simulation.Model;

public abstract class SimModule
{
    public abstract void Simulate(World world, Building building);
    public virtual  void Dispose (World world, Building building) { }
}
