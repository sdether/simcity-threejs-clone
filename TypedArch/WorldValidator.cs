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

internal static class WorldValidator
{
    private static readonly BindingFlags s_flags =
        BindingFlags.Instance | BindingFlags.Static |
        BindingFlags.Public   | BindingFlags.NonPublic;

public static ValidationReport Validate(
        Dictionary<Type, ArchetypeDefinition> definitions,
        IReadOnlyList<ISystem> systems)
    {
        var errors          = new List<string>();
        var warnings        = new List<string>();
        var allQueriedTypes = new HashSet<Type>();

        foreach (var system in systems)
        {
            foreach (var (query, fieldBinding) in GetQueries(system))
            {
                var withAll  = SignatureHelper.Types(query.All);
                var withAny  = SignatureHelper.Types(query.Any);
                var withNone = SignatureHelper.Types(query.None);

                allQueriedTypes.UnionWith(withAll);
                allQueriedTypes.UnionWith(withAny);
                allQueriedTypes.UnionWith(withNone);

                // General satisfiability.
                var satisfiable = definitions.Values.Any(def =>
                    withAll.IsSubsetOf(def.All) &&
                    (withAny.Count == 0 || withAny.Overlaps(def.All)) &&
                    !withNone.Overlaps(def.Required));

                if (!satisfiable)
                    errors.Add(
                        $"System '{system.GetType().Name}' has query " +
                        $"{DescribeQuery(withAll, withAny, withNone)} " +
                        $"that no archetype can satisfy — it will never match.");

                var archetypeType = fieldBinding;
                if (archetypeType is null) continue;

                ArchetypeDefinition boundDef;
                if (!definitions.TryGetValue(archetypeType, out var registeredDef))
                {
                    // Concrete archetypes (IArcheType) must be registered; abstract ones need not be.
                    if (typeof(IArcheType).IsAssignableFrom(archetypeType))
                    {
                        errors.Add(
                            $"System '{system.GetType().Name}' has a query bound to " +
                            $"'{archetypeType.Name}' which is not registered.");
                        continue;
                    }
                    boundDef = ArchetypeDefinition.ExtractFrom(archetypeType, -1);
                }
                else
                {
                    boundDef = registeredDef;
                }

                var undeclared = withAll.Concat(withAny)
                    .Where(t => !boundDef.All.Contains(t)).ToList();
                if (undeclared.Count > 0)
                    errors.Add(
                        $"System '{system.GetType().Name}' (bound to '{archetypeType.Name}') " +
                        $"queries component(s) [{Join(undeclared)}] not declared on that archetype.");

                var alwaysPresent = withNone.Where(t => boundDef.Required.Contains(t)).ToList();
                if (alwaysPresent.Count > 0)
                    errors.Add(
                        $"System '{system.GetType().Name}' (bound to '{archetypeType.Name}') " +
                        $"excludes component(s) [{Join(alwaysPresent)}] that are Required " +
                        $"— the query can never match.");
            }
        }

        foreach (var def in definitions.Values)
            foreach (var type in def.Optional.Where(t => !allQueriedTypes.Contains(t)))
                warnings.Add(
                    $"Optional component '{type.Name}' on archetype '{def.ArchetypeType.Name}' " +
                    $"does not appear in any system query.");

        return new ValidationReport(errors, warnings);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// Scans fields for QueryDescription and TypedQueryDescription&lt;T&gt;.
    /// Returns (query, archetypeType) — archetypeType is null for plain QueryDescription fields.
    private static IEnumerable<(QueryDescription query, Type? archetypeType)> GetQueries(ISystem system)
    {
        foreach (var field in system.GetType().GetFields(s_flags))
        {
            var value = field.GetValue(field.IsStatic ? null : system);
            if (value is null) continue;

            if (value is QueryDescription qd)
                yield return (qd, null);
            else if (value is ITypedQueryDescription tqd)
                yield return (tqd.Inner, tqd.ArchetypeType);
        }
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
