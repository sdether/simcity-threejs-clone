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
    private static QueryDescription _query = new QueryDescription().WithAll<CitizenInfo, Job>();
    public QueryDescription Query => _query;
    public IReadOnlySet<Type> Writes => [typeof(Job)];
    public void Run(TypedWorld world)
    {
        world.World.Query(in _query, (Entity entity, ref CitizenInfo citizenInfo, ref Job job) =>
        {

        });
    }
}

public class CitizenJobSystem : JobSystem, ISystem<ICitizen>
{
}