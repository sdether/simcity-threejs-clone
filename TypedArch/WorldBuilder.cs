namespace TypedArch;

public static class WorldBuilder
{
    public static WorldBuilder<TEnum> For<TEnum>() where TEnum : struct, Enum => new();
}

public class WorldBuilder<TEnum> where TEnum : struct, Enum
{
    private readonly Dictionary<int, ArchetypeDefinition<TEnum>> _definitions = new();
    private readonly List<TypedSystem>                           _systems     = [];

    // ── Archetypes ────────────────────────────────────────────────────────────

    public ArchetypeBuilder<TEnum> RegisterArchetype(TEnum id) =>
        new(this, id, null, [], []);

    /// <summary>
    /// Creates a child archetype pre-seeded with the parent's Required and Optional sets.
    /// Parent must already be registered.
    /// </summary>
    public ArchetypeBuilder<TEnum> Subclass(TEnum parentId, TEnum childId)
    {
        var key = Convert.ToInt32(parentId);
        if (!_definitions.TryGetValue(key, out var parent))
            throw new ArchetypeValidationException(
                $"Parent archetype '{parentId}' is not registered. Register it before calling Subclass.");

        return new ArchetypeBuilder<TEnum>(
            this, childId, parentId,
            new HashSet<Type>(parent.Required),
            new HashSet<Type>(parent.Optional));
    }

    internal void AddDefinition(ArchetypeDefinition<TEnum> def)
    {
        var key = Convert.ToInt32(def.Id);
        if (_definitions.ContainsKey(key))
            throw new ArchetypeValidationException($"Archetype '{def.Id}' is already registered.");
        _definitions[key] = def;
    }

    // ── Systems ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Registers a system for static analysis. Returns a SystemBuilder to declare write components.
    /// </summary>
    public SystemBuilder<TEnum> RegisterSystem<T>() where T : TypedSystem, new() =>
        new(this, new T());

    internal void AddSystem(TypedSystem system) => _systems.Add(system);

    // ── Build ─────────────────────────────────────────────────────────────────

    public TypedWorld<TEnum> BuildWorld()
    {
        ValidateExhaustiveness();
        return new TypedWorld<TEnum>(_definitions, _systems);
    }

    private void ValidateExhaustiveness()
    {
        var missing = Enum.GetValues<TEnum>()
            .Where(v => !_definitions.ContainsKey(Convert.ToInt32(v)))
            .Select(v => v.ToString())
            .ToList();

        if (missing.Count > 0)
            throw new ArchetypeValidationException(
                $"The following archetype(s) have no registered definition: {string.Join(", ", missing)}");
    }
}
