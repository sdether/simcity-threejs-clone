namespace TypedArch;

public class ArchetypeBuilder<TEnum> where TEnum : struct, Enum
{
    private readonly WorldBuilder<TEnum> _worldBuilder;
    private readonly TEnum               _id;
    private readonly TEnum?              _parentId;
    private readonly HashSet<Type>       _required;
    private readonly HashSet<Type>       _optional;
    private readonly HashSet<Type>       _ownRequired = [];
    private readonly HashSet<Type>       _ownOptional = [];

    internal ArchetypeBuilder(
        WorldBuilder<TEnum> worldBuilder, TEnum id, TEnum? parentId,
        HashSet<Type> inheritedRequired, HashSet<Type> inheritedOptional)
    {
        _worldBuilder = worldBuilder;
        _id           = id;
        _parentId     = parentId;
        _required     = new HashSet<Type>(inheritedRequired);
        _optional     = new HashSet<Type>(inheritedOptional);
    }

    public ArchetypeBuilder<TEnum> Requires<T>() where T : struct
    {
        _required.Add(typeof(T));
        _ownRequired.Add(typeof(T));
        return this;
    }

    public ArchetypeBuilder<TEnum> Allows<T>() where T : struct
    {
        _optional.Add(typeof(T));
        _ownOptional.Add(typeof(T));
        return this;
    }

    public WorldBuilder<TEnum> BuildArchetype()
    {
        var def = new ArchetypeDefinition<TEnum>(
            _id, _parentId,
            _required, _optional,
            _ownRequired, _ownOptional);
        _worldBuilder.AddDefinition(def);
        return _worldBuilder;
    }
}
