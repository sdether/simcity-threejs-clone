namespace TypedArch;

public class TypedWorldBuilder
{
    private readonly Dictionary<Type, ArchetypeDefinition> _definitions = new();
    private readonly List<ArchetypeDefinition>             _index       = new();
    private readonly List<ISystem>                         _systems     = [];

    public TypedWorldBuilder RegisterArcheType<T>() where T : IArcheType
    {
        var type = typeof(T);
        if (_definitions.ContainsKey(type))
            throw new ArchetypeValidationException($"Archetype '{type.Name}' is already registered.");

        var def = ArchetypeDefinition.ExtractFrom(type, _index.Count);
        _definitions[type] = def;
        _index.Add(def);
        return this;
    }

    public TypedWorldBuilder RegisterSystem<T>() where T : ISystem, new()
    {
        _systems.Add(new T());
        return this;
    }

    public TypedWorld Build()
    {
        return new TypedWorld(_definitions, _index, _systems);
    }
}
