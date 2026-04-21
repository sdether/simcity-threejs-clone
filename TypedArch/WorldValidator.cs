using System.Reflection;
using Arch.Core;

namespace TypedArch;

internal static class SignatureHelper
{
    internal static HashSet<Type> Types(Signature sig)
    {
        var span   = sig.Components;
        var result = new HashSet<Type>(span.Length);
        for (var i = 0; i < span.Length; i++)
            result.Add(span[i].Type);
        return result;
    }
}

internal static class WorldValidator<TEnum> where TEnum : struct, Enum
{
    // The open generic definition used for isinstance checks.
    private static readonly Type s_entitySystemOpenType = typeof(TypedEntitySystem<>);

    public static ValidationReport Validate(
        Dictionary<int, ArchetypeDefinition<TEnum>> definitions,
        IReadOnlyList<TypedSystem> systems)
    {
        var errors   = new List<string>();
        var warnings = new List<string>();

        foreach (var system in systems)
        {
            ValidateQueries(system, definitions, errors);
            ValidateWrites(system, errors);
            ValidateEntitySystem(system, definitions, errors);
        }

        WarnDeadOptionals(systems, definitions, warnings);

        return new ValidationReport(errors, warnings);
    }

    // ── Per-system checks ─────────────────────────────────────────────────────

    private static void ValidateQueries(
        TypedSystem system,
        Dictionary<int, ArchetypeDefinition<TEnum>> definitions,
        List<string> errors)
    {
        foreach (var query in system.GetQueries())
        {
            var withAll  = SignatureHelper.Types(query.All);
            var withAny  = SignatureHelper.Types(query.Any);
            var withNone = SignatureHelper.Types(query.None);

            // General satisfiability: at least one archetype could produce a matching entity.
            var satisfiable = definitions.Values.Any(def =>
                withAll.IsSubsetOf(def.All) &&
                (withAny.Count == 0 || withAny.Overlaps(def.All)) &&
                !withNone.Overlaps(def.Required));

            if (!satisfiable)
                errors.Add(
                    $"System '{system.GetType().Name}' has a query {DescribeQuery(withAll, withAny, withNone)} " +
                    $"that no archetype can satisfy — it will never match.");
        }
    }

    private static void ValidateWrites(TypedSystem system, List<string> errors)
    {
        var allReads = system.ReadComponents;
        foreach (var write in system.WriteComponents.Where(w => !allReads.Contains(w)))
            errors.Add(
                $"System '{system.GetType().Name}' declares write for '{write.Name}' " +
                $"but that component does not appear in any of its queries.");
    }

    private static void ValidateEntitySystem(
        TypedSystem system,
        Dictionary<int, ArchetypeDefinition<TEnum>> definitions,
        List<string> errors)
    {
        var enumArg = GetEntitySystemEnumArgument(system);
        if (enumArg is null) return; // not a TypedEntitySystem<>

        // (a) TEnum must match the world's TEnum.
        if (enumArg != typeof(TEnum))
        {
            errors.Add(
                $"System '{system.GetType().Name}' is TypedEntitySystem<{enumArg.Name}> " +
                $"but this world uses {typeof(TEnum).Name}. The archetype enum types must match.");
            return; // can't do archetype check without a matching enum
        }

        // (b) Query components must conform to the declared archetype.
        var archeType  = (TEnum)system.GetType().GetProperty("ArcheType")!.GetValue(system)!;
        var archetypeKey = Convert.ToInt32(archeType);

        if (!definitions.TryGetValue(archetypeKey, out var def))
        {
            errors.Add(
                $"System '{system.GetType().Name}' declares ArcheType '{archeType}' " +
                $"which is not registered in this world.");
            return;
        }

        foreach (var query in system.GetQueries())
        {
            var withAll  = SignatureHelper.Types(query.All);
            var withAny  = SignatureHelper.Types(query.Any);
            var withNone = SignatureHelper.Types(query.None);

            // WithAll/WithAny components must be declared on the archetype.
            var undeclared = withAll.Concat(withAny)
                .Where(t => !def.All.Contains(t))
                .ToList();
            if (undeclared.Count > 0)
                errors.Add(
                    $"System '{system.GetType().Name}' (ArcheType '{archeType}') queries " +
                    $"component(s) [{Join(undeclared)}] that are not declared on that archetype.");

            // WithNone components that are Required can never be absent — query can never match.
            var alwaysPresent = withNone.Where(t => def.Required.Contains(t)).ToList();
            if (alwaysPresent.Count > 0)
                errors.Add(
                    $"System '{system.GetType().Name}' (ArcheType '{archeType}') excludes " +
                    $"component(s) [{Join(alwaysPresent)}] that are Required on that archetype " +
                    $"— the query can never match.");
        }
    }

    // ── World-level checks ────────────────────────────────────────────────────

    private static void WarnDeadOptionals(
        IReadOnlyList<TypedSystem> systems,
        Dictionary<int, ArchetypeDefinition<TEnum>> definitions,
        List<string> warnings)
    {
        var allQueried = systems
            .SelectMany(s => s.GetQueries())
            .SelectMany(q => SignatureHelper.Types(q.All)
                .Concat(SignatureHelper.Types(q.Any))
                .Concat(SignatureHelper.Types(q.None)))
            .ToHashSet();

        foreach (var def in definitions.Values)
            foreach (var type in def.Optional.Where(t => !allQueried.Contains(t)))
                warnings.Add(
                    $"Optional component '{type.Name}' on archetype '{def.Id}' " +
                    $"does not appear in any system query.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// Returns the TEnum generic argument if the system extends TypedEntitySystem<TEnum>,
    /// or null if it does not.
    private static Type? GetEntitySystemEnumArgument(TypedSystem system)
    {
        var type = system.GetType().BaseType;
        while (type is not null)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == s_entitySystemOpenType)
                return type.GetGenericArguments()[0];
            type = type.BaseType;
        }
        return null;
    }

    private static string DescribeQuery(HashSet<Type> all, HashSet<Type> any, HashSet<Type> none)
    {
        var parts = new List<string>();
        if (all.Count  > 0) parts.Add($"WithAll<{Join(all)}>");
        if (any.Count  > 0) parts.Add($"WithAny<{Join(any)}>");
        if (none.Count > 0) parts.Add($"WithNone<{Join(none)}>");
        return $"[{string.Join(", ", parts)}]";
    }

    private static string Join(IEnumerable<Type> types) =>
        string.Join(", ", types.Select(t => t.Name));
}
