using System.Reflection;
using Arch.Core;
using Arch.Core.Extensions;
using EntityExtensions = Arch.Core.Extensions.EntityExtensions;

namespace TypedArch;

public class TypedWorld<TEnum> : IDisposable where TEnum : struct, Enum
{
    /// <summary>Raw Arch world — use this for queries.</summary>
    public World World { get; } = World.Create();

    private readonly Dictionary<int, ArchetypeDefinition<TEnum>> _definitions;
    private readonly IReadOnlyList<TypedSystem>                   _systems;

    // Cached MethodInfo for EntityExtensions.Add<T>(ref Entity, in T).
    private static readonly MethodInfo s_addMethod =
        typeof(EntityExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == "Add"
                && m.IsGenericMethodDefinition
                && m.GetGenericArguments().Length == 1
                && m.GetParameters().Length == 2);

    internal TypedWorld(Dictionary<int, ArchetypeDefinition<TEnum>> definitions, IReadOnlyList<TypedSystem> systems)
    {
        _definitions = definitions;
        _systems     = systems;
    }

    // ── Entity creation ───────────────────────────────────────────────────────

    /// <summary>
    /// Creates an entity for the given archetype.
    /// Throws if any required component is missing or an undeclared component is included.
    /// </summary>
    public Entity Create(TEnum archetype, params object[] components)
    {
        var def = GetDefinition(archetype);
        ValidateComponents(def, components);

        var entity = World.Create(new ArchetypeRef(Convert.ToInt32(archetype)));
        foreach (var component in components)
            AddBoxed(entity, component);

        return entity;
    }

    // ── Component mutation ────────────────────────────────────────────────────

    public void Add<T>(Entity entity, T component) where T : struct
    {
        var def = GetDefinitionForEntity(entity);
        if (!def.All.Contains(typeof(T)))
            throw new ArchetypeValidationException(
                $"Component '{typeof(T).Name}' is not permitted on archetype '{def.Id}'.");
        entity.Add(component);
    }

    public void Add<T>(Entity entity) where T : struct
    {
        var def = GetDefinitionForEntity(entity);
        if (!def.All.Contains(typeof(T)))
            throw new ArchetypeValidationException(
                $"Component '{typeof(T).Name}' is not permitted on archetype '{def.Id}'.");
        entity.Add<T>();
    }

    public void Remove<T>(Entity entity) where T : struct
    {
        var def = GetDefinitionForEntity(entity);
        if (def.Required.Contains(typeof(T)))
            throw new ArchetypeValidationException(
                $"Cannot remove required component '{typeof(T).Name}' from archetype '{def.Id}'.");
        entity.Remove<T>();
    }

    // ── Other world operations ────────────────────────────────────────────────

    public TEnum GetArchetype(Entity entity) =>
        (TEnum)(object)entity.Get<ArchetypeRef>().ArchetypeId;

    public void Destroy(Entity entity) => World.Destroy(entity);

    /// <summary>Runs static analysis against the registered systems.</summary>
    public ValidationReport Validate() =>
        WorldValidator<TEnum>.Validate(_definitions, _systems);

    /// <summary>Runs static analysis against the provided systems (useful in tests).</summary>
    public ValidationReport Validate(IReadOnlyList<TypedSystem> systems) =>
        WorldValidator<TEnum>.Validate(_definitions, systems);

    public void Dispose() => World.Dispose();

    // ── Helpers ───────────────────────────────────────────────────────────────

    private ArchetypeDefinition<TEnum> GetDefinition(TEnum archetype) =>
        _definitions[Convert.ToInt32(archetype)];

    private ArchetypeDefinition<TEnum> GetDefinitionForEntity(Entity entity)
    {
        var id = entity.Get<ArchetypeRef>().ArchetypeId;
        return _definitions[id];
    }

    private static void ValidateComponents(ArchetypeDefinition<TEnum> def, object[] components)
    {
        var provided = components.Select(c => c.GetType()).ToHashSet();

        var missing = def.Required.Except(provided).ToList();
        if (missing.Count > 0)
            throw new ArchetypeValidationException(
                $"Archetype '{def.Id}' is missing required component(s): " +
                string.Join(", ", missing.Select(t => t.Name)));

        var extra = provided.Except(def.All).ToList();
        if (extra.Count > 0)
            throw new ArchetypeValidationException(
                $"Component(s) not permitted on archetype '{def.Id}': " +
                string.Join(", ", extra.Select(t => t.Name)));
    }

    private static void AddBoxed(Entity entity, object component) =>
        s_addMethod.MakeGenericMethod(component.GetType()).Invoke(null, [entity, component]);
}
