using System.Reflection;

namespace TypedArch;

/// <summary>
/// Stamped on every entity at creation so the archetype can be resolved from an entity.
/// </summary>
public readonly record struct ArchetypeRef(int Index);

public class ArchetypeDefinition
{
    public Type                ArchetypeType { get; }
    public int                 Index         { get; }
    public IReadOnlySet<Type>  Required      { get; }
    public IReadOnlySet<Type>  Optional      { get; }
    public IReadOnlySet<Type>  All           { get; }
    public IReadOnlyList<Type> ParentTypes   { get; }

    private ArchetypeDefinition(Type archetypeType, int index,
        HashSet<Type> required, HashSet<Type> optional, List<Type> parentTypes)
    {
        ArchetypeType = archetypeType;
        Index         = index;
        Required      = required;
        Optional      = optional;
        All           = required.Union(optional).ToHashSet();
        ParentTypes   = parentTypes;
    }

    internal static ArchetypeDefinition ExtractFrom(Type archetypeType, int index)
    {
        if (!archetypeType.IsInterface)
            throw new ArchetypeValidationException(
                $"'{archetypeType.Name}' must be an interface to be used as an archetype.");

        var required    = new HashSet<Type>();
        var optional    = new HashSet<Type>();
        var parentTypes = new List<Type>();

        // All parent interfaces except the two marker interfaces themselves — both plain and
        // IAbstractArcheType-implementing base interfaces are valid component sources.
        var allParents = archetypeType.GetInterfaces()
            .Where(i => i != typeof(IArcheType) && i != typeof(IAbstractArcheType))
            .ToHashSet();

        // IArcheType-implementing ancestors only — for parent type tracking in the validator.
        var allAncestors = allParents
            .Where(i => typeof(IArcheType).IsAssignableFrom(i))
            .ToHashSet();

        foreach (var candidate in allAncestors)
        {
            // A direct parent is one that no other ancestor also extends.
            var isDirect = !allAncestors.Any(other =>
                other != candidate && other.GetInterfaces().Contains(candidate));
            if (isDirect) parentTypes.Add(candidate);
        }

        // Collect properties from this interface and all parent interfaces.
        foreach (var iface in allParents.Prepend(archetypeType))
        {
            foreach (var prop in iface.GetProperties(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                var underlying = Nullable.GetUnderlyingType(prop.PropertyType);
                if (underlying != null)
                    optional.Add(underlying);
                else
                    required.Add(prop.PropertyType);
            }
        }

        return new ArchetypeDefinition(archetypeType, index, required, optional, parentTypes);
    }
}
