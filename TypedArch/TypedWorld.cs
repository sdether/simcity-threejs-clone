using Arch.Core;

namespace TypedArch;

public class TypedWorld<TArchetypes, TSystems> : IDisposable
{
    public World Inner { get; } = World.Create();

    private readonly Dictionary<Type, ArchetypeDefinition> _byType;
    private readonly IReadOnlyList<ISystem>                _systems;

    public TypedWorld()
    {
        _byType  = RegisterArchetypes();
        _systems = RegisterSystems();
    }

    public ValidationReport Validate() =>
        WorldValidator.Validate(_byType, _systems);

    public ValidationReport Validate(IReadOnlyList<ISystem> systems) =>
        WorldValidator.Validate(_byType, systems);

    public void Dispose() => Inner.Dispose();

    // ── Registration ──────────────────────────────────────────────────────────

    private static Dictionary<Type, ArchetypeDefinition> RegisterArchetypes()
    {
        var byType = new Dictionary<Type, ArchetypeDefinition>();

        foreach (var prop in typeof(TArchetypes).GetProperties().OrderBy(p => p.MetadataToken))
        {
            var type = prop.PropertyType;
            if (!typeof(IArcheType).IsAssignableFrom(type)) continue;
            if (!type.IsInterface)
                throw new ArchetypeValidationException(
                    $"'{type.Name}' must be an interface to be registered as an archetype.");
            if (byType.ContainsKey(type))
                throw new ArchetypeValidationException(
                    $"Archetype '{type.Name}' appears more than once in {typeof(TArchetypes).Name}.");

            byType[type] = ArchetypeDefinition.ExtractFrom(type);
        }

        return byType;
    }

    private static List<ISystem> RegisterSystems()
    {
        var systems = new List<ISystem>();

        foreach (var prop in typeof(TSystems).GetProperties().OrderBy(p => p.MetadataToken))
        {
            var type = prop.PropertyType;
            if (type.IsInterface || !typeof(ISystem).IsAssignableFrom(type)) continue;

            var instance = Activator.CreateInstance(type) as ISystem
                ?? throw new ArchetypeValidationException(
                    $"Could not instantiate system '{type.Name}'. Ensure it has a public parameterless constructor.");
            systems.Add(instance);
        }

        return systems;
    }
}
