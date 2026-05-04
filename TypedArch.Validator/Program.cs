using System.Reflection;
using System.Runtime.Loader;
using TypedArch;

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: TypedArch.Validator <path-to-assembly>");
    return 2;
}

var assemblyPath = Path.GetFullPath(args[0]);

Assembly assembly;
try
{
    var ctx = new TargetAssemblyLoadContext(assemblyPath);
    assembly = ctx.LoadFromAssemblyPath(assemblyPath);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Failed to load '{assemblyPath}': {ex.Message}");
    return 2;
}

// Discover every distinct TypedWorld<TArchetypes, TSystems> declared as a field or
// property anywhere in the assembly, and validate each one.
var typedWorldBase = typeof(TypedWorld<,>);
const BindingFlags allMembers = BindingFlags.Instance | BindingFlags.Static |
                                BindingFlags.Public   | BindingFlags.NonPublic;

var discovered = (
    from type in assembly.GetTypes()
    from member in type.GetMembers(allMembers)
    let memberType = member is FieldInfo    f ? f.FieldType
                   : member is PropertyInfo p ? p.PropertyType
                   : null
    where memberType is { IsGenericType: true }
       && memberType.GetGenericTypeDefinition() == typedWorldBase
    select memberType.GetGenericArguments()
).Distinct(TypeArgArrayEqualityComparer.Instance).ToList();

if (discovered.Count == 0)
{
    Console.Error.WriteLine(
        $"No TypedWorld<,> field or property found in '{Path.GetFileName(assemblyPath)}'.");
    return 2;
}

var allValid = true;

foreach (var typeArgs in discovered)
{
    Console.WriteLine($"Validating TypedWorld<{typeArgs[0].Name}, {typeArgs[1].Name}>...");
    try
    {
        var worldType = typedWorldBase.MakeGenericType(typeArgs);
        using var world = (IDisposable)Activator.CreateInstance(worldType)!;
        var report      = (ValidationReport)worldType.GetMethod("Validate", Type.EmptyTypes)!
                              .Invoke(world, null)!;

        foreach (var warning in report.Warnings)
            Console.WriteLine($"  warning: {warning}");
        foreach (var error in report.Errors)
            Console.Error.WriteLine($"  error: {error}");

        if (report.IsValid)
            Console.WriteLine("  Passed.");
        else
        {
            Console.Error.WriteLine("  Failed.");
            allValid = false;
        }
    }
    catch (Exception ex)
    {
        var inner = ex;
        while (inner.InnerException != null) inner = inner.InnerException;
        Console.Error.WriteLine($"  Threw: {inner.GetType().Name}: {inner.Message}");
        Console.Error.WriteLine(inner.StackTrace);
        allValid = false;
    }
}

return allValid ? 0 : 1;

sealed class TypeArgArrayEqualityComparer : IEqualityComparer<Type[]>
{
    public static readonly TypeArgArrayEqualityComparer Instance = new();
    public bool Equals(Type[]? x, Type[]? y)  => x != null && y != null && x.SequenceEqual(y);
    public int  GetHashCode(Type[] a)          => a.Aggregate(0, (h, t) => HashCode.Combine(h, t));
}

/// <summary>
/// Loads the target assembly's dependencies from its own output directory while
/// keeping TypedArch and Arch bound to the validator's already-loaded copies so
/// that shared types (ValidationReport, IArcheType, etc.) have a single identity.
/// </summary>
class TargetAssemblyLoadContext(string assemblyPath) : AssemblyLoadContext(isCollectible: false)
{
    private readonly AssemblyDependencyResolver _resolver = new(assemblyPath);

    private static readonly HashSet<string> s_sharedWithValidator = ["TypedArch", "Arch"];

    protected override Assembly? Load(AssemblyName name)
    {
        if (s_sharedWithValidator.Contains(name.Name ?? "")) return null; // use validator's copy
        var path = _resolver.ResolveAssemblyToPath(name);
        return path is not null ? LoadFromAssemblyPath(path) : null;
    }
}
