using Arch.Core;
using Simulation.Components;
using TypedArch;


namespace Simulation;

public interface ICitizen : IArcheType
{
    CitizenInfo CitizenInfo { get; }
    CitizenStatus CitizenStatus { get; }
    Residency? Residency { get; }
    Job? Job { get; }
}

public class JobSystem : ISystem
{
    private static readonly TypedQueryDescription<ICitizen> _query = TypedQueryDescription
        .Create<ICitizen>()
        .WithAll<CitizenInfo, Job>();
    
    public void Run(World world)
    {
        world.Query(in _query.Inner, (Entity entity, ref CitizenInfo citizenInfo, ref Job job) =>
        {

        });
    }
}


public interface IArcheTypes
{
    ICitizen Citizen { get; }
}

public interface ISystems
{
    JobSystem JobSystem { get; }
}

public static class Env {
    public static void Run()
    {
        var typedWorld = new TypedWorld<IArcheTypes, ISystems>();
    }
}