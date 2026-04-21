namespace TypedArch;

/// <summary>
/// Stamped on every entity by TypedWorld.Create so the archetype can be resolved
/// from an entity without a separate dictionary.
/// </summary>
public readonly record struct ArchetypeRef(int ArchetypeId);

public class ArchetypeDefinition<TEnum> where TEnum : struct, Enum
{
    public TEnum               Id          { get; }
    public TEnum?              ParentId    { get; }
    public IReadOnlySet<Type>  Required    { get; }   // includes inherited
    public IReadOnlySet<Type>  Optional    { get; }   // includes inherited
    public IReadOnlySet<Type>  OwnRequired { get; }   // declared on this level only
    public IReadOnlySet<Type>  OwnOptional { get; }
    public IReadOnlySet<Type>  All         { get; }   // Required ∪ Optional

    internal ArchetypeDefinition(
        TEnum id, TEnum? parentId,
        HashSet<Type> required, HashSet<Type> optional,
        HashSet<Type> ownRequired, HashSet<Type> ownOptional)
    {
        Id          = id;
        ParentId    = parentId;
        Required    = required;
        Optional    = optional;
        OwnRequired = ownRequired;
        OwnOptional = ownOptional;
        All         = required.Union(optional).ToHashSet();
    }
}
