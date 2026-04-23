using System.Reflection;
using Arch.Core;
using Arch.Core.Extensions;
using EntityExtensions = Arch.Core.Extensions.EntityExtensions;

namespace TypedArch;

public class TypedWorld<TArchetypes, TSystems> : IDisposable
{
    /// <summary>Raw Arch world — use this for queries.</summary>
    public World World { get; } = World.Create();

    private readonly Dictionary<Type, ArchetypeDefinition> _byType;
    private readonly List<ArchetypeDefinition>             _byIndex;
    private readonly IReadOnlyList<ISystem>                _systems;

    private static readonly MethodInfo s_addMethod =
        typeof(EntityExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == "Add"
                && m.IsGenericMethodDefinition
                && m.GetGenericArguments().Length == 1
                && m.GetParameters().Length == 2);

    public TypedWorld()
    {
        (_byType, _byIndex) = RegisterArchetypes();
        _systems             = RegisterSystems();
    }

    // ── Entity creation ───────────────────────────────────────────────────────

    public Entity Create<TArcheType>(params object[] components) where TArcheType : IArcheType
    {
        if (!_byType.TryGetValue(typeof(TArcheType), out var def))
            throw new ArchetypeValidationException(
                $"Archetype '{typeof(TArcheType).Name}' is not registered.");

        ValidateComponents(def, components);

        var entity = World.Create(new ArchetypeRef(def.Index));
        foreach (var component in components)
            AddBoxed(entity, component);

        return entity;
    }

    // ── Component mutation ────────────────────────────────────────────────────

    public void Add<T>(Entity entity, T component) where T : struct
    {
#if DEBUG
        var def = GetDefinitionForEntity(entity);
        if (!def.All.Contains(typeof(T)))
            throw new ArchetypeValidationException(
                $"Component '{typeof(T).Name}' is not permitted on archetype '{def.ArchetypeType.Name}'.");
#endif
        entity.Add(component);
    }

    public void Add<T>(Entity entity) where T : struct
    {
#if DEBUG
        var def = GetDefinitionForEntity(entity);
        if (!def.All.Contains(typeof(T)))
            throw new ArchetypeValidationException(
                $"Component '{typeof(T).Name}' is not permitted on archetype '{def.ArchetypeType.Name}'.");
#endif
        entity.Add<T>();
    }

    public void Remove<T>(Entity entity) where T : struct
    {
#if DEBUG
        var def = GetDefinitionForEntity(entity);
        if (def.Required.Contains(typeof(T)))
            throw new ArchetypeValidationException(
                $"Cannot remove required component '{typeof(T).Name}' from archetype '{def.ArchetypeType.Name}'.");
#endif
        entity.Remove<T>();
    }

    // ── Other world operations ────────────────────────────────────────────────

    public Type GetArchetype(Entity entity) =>
        _byIndex[entity.Get<ArchetypeRef>().Index].ArchetypeType;

    public void Destroy(Entity entity) => World.Destroy(entity);

    public ValidationReport Validate() =>
        WorldValidator.Validate(_byType, _systems);

    public ValidationReport Validate(IReadOnlyList<ISystem> systems) =>
        WorldValidator.Validate(_byType, systems);

    public void Dispose() => World.Dispose();

    // ── Registration ──────────────────────────────────────────────────────────

    private static (Dictionary<Type, ArchetypeDefinition>, List<ArchetypeDefinition>) RegisterArchetypes()
    {
        var byType  = new Dictionary<Type, ArchetypeDefinition>();
        var byIndex = new List<ArchetypeDefinition>();

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

            var def = ArchetypeDefinition.ExtractFrom(type, byIndex.Count);
            byType[type] = def;
            byIndex.Add(def);
        }

        return (byType, byIndex);
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

    // ── Helpers ───────────────────────────────────────────────────────────────

    private ArchetypeDefinition GetDefinitionForEntity(Entity entity) =>
        _byIndex[entity.Get<ArchetypeRef>().Index];

    private static void ValidateComponents(ArchetypeDefinition def, object[] components)
    {
        var provided = components.Select(c => c.GetType()).ToHashSet();

        var missing = def.Required.Except(provided).ToList();
        if (missing.Count > 0)
            throw new ArchetypeValidationException(
                $"Archetype '{def.ArchetypeType.Name}' is missing required component(s): " +
                string.Join(", ", missing.Select(t => t.Name)));

        var extra = provided.Except(def.All).ToList();
        if (extra.Count > 0)
            throw new ArchetypeValidationException(
                $"Component(s) not permitted on archetype '{def.ArchetypeType.Name}': " +
                string.Join(", ", extra.Select(t => t.Name)));
    }

    private static void AddBoxed(Entity entity, object component) =>
        s_addMethod.MakeGenericMethod(component.GetType()).Invoke(null, [entity, component]);
}
