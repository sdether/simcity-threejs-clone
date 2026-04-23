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
    CompA  CompA { get; }
    CompB? CompB { get; }
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

// ── Manifests ─────────────────────────────────────────────────────────────────

public interface ITestArchetypes
{
    IPrimary   Primary   { get; }
    ISecondary Secondary { get; }
    ITertiary  Tertiary  { get; }
}

public interface ITestSystems { }

// ── Standard world fixture ────────────────────────────────────────────────────

public static class TestWorld
{
    public static TypedWorld<ITestArchetypes, ITestSystems> Build() => new();
}

// ── Test systems ──────────────────────────────────────────────────────────────

public class ValidSystem : ISystem
{
    private static readonly QueryDescription _query =
        new QueryDescription().WithAll<CompA, CompB>();

    public void Run(World world) { }
}

public class CompAOnlySystem : ISystem
{
    private static readonly QueryDescription _query =
        new QueryDescription().WithAll<CompA>();

    public void Run(World world) { }
}

public class UnsatisfiableSystem : ISystem
{
    private static readonly QueryDescription _query =
        new QueryDescription().WithAll<CompA, CompD>();

    public void Run(World world) { }
}

public class ValidBoundSystem : ISystem
{
    private static readonly TypedQueryDescription<IPrimary> _query =
        TypedQueryDescription.Create<IPrimary>().WithAll<CompA>();

    public void Run(World world) { }
}

public class UndeclaredComponentBoundSystem : ISystem
{
    private static readonly TypedQueryDescription<IPrimary> _query =
        TypedQueryDescription.Create<IPrimary>().WithAll<CompD>();

    public void Run(World world) { }
}

public class ExcludesRequiredBoundSystem : ISystem
{
    private static readonly TypedQueryDescription<IPrimary> _query =
        TypedQueryDescription.Create<IPrimary>().WithAll<CompB>().WithNone<CompA>();

    public void Run(World world) { }
}

public class UnregisteredArchetypeSystem : ISystem
{
    private static readonly TypedQueryDescription<IUnregistered> _query =
        TypedQueryDescription.Create<IUnregistered>().WithAll<CompA>();

    public void Run(World world) { }
}
