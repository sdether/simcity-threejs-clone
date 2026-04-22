using Arch.Core;
using TypedArch;

namespace TypedArch.Tests;

// ── Test components ───────────────────────────────────────────────────────────

public struct CompA;
public struct CompB;
public struct CompC;
public struct CompD; // intentionally not declared on any archetype

// ── Test archetypes ───────────────────────────────────────────────────────────

public interface IPrimary : IArcheType
{
    CompA  CompA  { get; }
    CompB? CompB  { get; }
}

public interface ISecondary : IArcheType
{
    CompB CompB { get; }
    CompC CompC { get; }
}

public interface ITertiary : IPrimary
{
    CompC CompC { get; }
}

public interface IUnregistered : IArcheType
{
    CompA CompA { get; }
}

// ── Standard world fixture ────────────────────────────────────────────────────

public static class TestWorld
{
    public static TypedWorld Build() =>
        new TypedWorldBuilder()
            .RegisterArcheType<IPrimary>()
            .RegisterArcheType<ISecondary>()
            .RegisterArcheType<ITertiary>()
            .Build();
}

// ── Test systems ──────────────────────────────────────────────────────────────

public class ValidSystem : ISystem
{
    private static readonly QueryDescription _query =
        new QueryDescription().WithAll<CompA, CompB>();

    public void Run(TypedWorld world) { }
}

public class CompAOnlySystem : ISystem
{
    private static readonly QueryDescription _query =
        new QueryDescription().WithAll<CompA>();

    public void Run(TypedWorld world) { }
}

public class UnsatisfiableSystem : ISystem
{
    // CompD is on no archetype — will never match.
    private static readonly QueryDescription _query =
        new QueryDescription().WithAll<CompA, CompD>();

    public void Run(TypedWorld world) { }
}

public class ValidBoundSystem : ISystem
{
    private static readonly TypedQueryDescription<IPrimary> _query =
        TypedQueryDescription.Create<IPrimary>().WithAll<CompA>();

    public void Run(TypedWorld world) { }
}

public class UndeclaredComponentBoundSystem : ISystem
{
    // CompD is not declared on IPrimary.
    private static readonly TypedQueryDescription<IPrimary> _query =
        TypedQueryDescription.Create<IPrimary>().WithAll<CompD>();

    public void Run(TypedWorld world) { }
}

public class ExcludesRequiredBoundSystem : ISystem
{
    // CompA is Required on IPrimary — excluding it means the query can never match.
    private static readonly TypedQueryDescription<IPrimary> _query =
        TypedQueryDescription.Create<IPrimary>().WithAll<CompB>().WithNone<CompA>();

    public void Run(TypedWorld world) { }
}

public class UnregisteredArchetypeSystem : ISystem
{
    private static readonly TypedQueryDescription<IUnregistered> _query =
        TypedQueryDescription.Create<IUnregistered>().WithAll<CompA>();

    public void Run(TypedWorld world) { }
}

public class WritesBSystem : ISystem
{
    // Intentionally no field — used to test a system with no queries.
    public void Run(TypedWorld world) { }
}
