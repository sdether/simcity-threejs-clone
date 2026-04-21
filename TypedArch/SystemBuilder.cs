namespace TypedArch;

public class SystemBuilder<TEnum> where TEnum : struct, Enum
{
    private readonly WorldBuilder<TEnum> _worldBuilder;
    private readonly TypedSystem         _system;
    private readonly HashSet<Type>       _writes = [];

    internal SystemBuilder(WorldBuilder<TEnum> worldBuilder, TypedSystem system)
    {
        _worldBuilder = worldBuilder;
        _system       = system;
    }

    /// <summary>
    /// Declares that this system mutates component T.
    /// T must appear in WithAll or WithAny of at least one of the system's QueryDescription members.
    /// </summary>
    public SystemBuilder<TEnum> Writes<T>() where T : struct
    {
        if (!_system.ReadComponents.Contains(typeof(T)))
            throw new ArchetypeValidationException(
                $"System '{_system.GetType().Name}' cannot declare write for '{typeof(T).Name}': " +
                $"it is not in the system's read component set.");
        _writes.Add(typeof(T));
        return this;
    }

    public WorldBuilder<TEnum> BuildSystem()
    {
        _system.SetWriteComponents(_writes);
        _worldBuilder.AddSystem(_system);
        return _worldBuilder;
    }
}
