using System.Reflection;
using Arch.Core;

namespace TypedArch;

/// <summary>
/// Base class for all systems. Declare QueryDescription fields or properties (any
/// visibility, static or instance) and the framework discovers them automatically
/// for static analysis and write-component validation.
/// </summary>
public abstract class TypedSystem
{
    public abstract void Run(World world);

    internal IReadOnlySet<Type> WriteComponents { get; private set; } = new HashSet<Type>();
    internal void SetWriteComponents(HashSet<Type> writes) => WriteComponents = writes;

    internal IEnumerable<QueryDescription> GetQueries()
    {
        var flags = BindingFlags.Instance | BindingFlags.Static |
                    BindingFlags.Public   | BindingFlags.NonPublic;

        return GetType().GetFields(flags)
                   .Where(f => f.FieldType == typeof(QueryDescription))
                   .Select(f => (QueryDescription)f.GetValue(this)!)
               .Concat(
                   GetType().GetProperties(flags)
                       .Where(p => p.PropertyType == typeof(QueryDescription))
                       .Select(p => (QueryDescription)p.GetValue(this)!));
    }

    /// <summary>
    /// All component types that appear in WithAll or WithAny across every query on this system.
    /// Used by SystemBuilder to validate Writes declarations.
    /// </summary>
    internal IReadOnlySet<Type> ReadComponents =>
        GetQueries()
            .SelectMany(q => SignatureHelper.Types(q.All).Concat(SignatureHelper.Types(q.Any)))
            .ToHashSet();
}

public abstract class TypedEntitySystem<TEnum> : TypedSystem where TEnum : struct, Enum
{

    public TEnum ArcheType { get; }
    protected TypedEntitySystem(TEnum archetypeId) => ArcheType = archetypeId;
}