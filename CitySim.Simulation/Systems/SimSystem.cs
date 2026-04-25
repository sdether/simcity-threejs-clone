using Arch.Core;
using World = CitySim.Simulation.Model.World;

namespace CitySim.Simulation.Systems;

public abstract class SimSystem
{
    protected readonly QueryDescription QueryDescription;
    
    protected SimSystem(QueryDescription queryDescription)
    {
        QueryDescription = queryDescription;
    }
    
    public abstract void Run(World world);
}
