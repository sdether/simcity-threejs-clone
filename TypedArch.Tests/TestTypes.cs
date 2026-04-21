using Arch.Core;
using TypedArch;

namespace TypedArch.Tests;

// ── Test enum ─────────────────────────────────────────────────────────────────

public enum TestEnum { Primary, Secondary, Tertiary }
public enum OtherEnum { X }

// ── Test components ───────────────────────────────────────────────────────────

public struct CompA;
public struct CompB;
public struct CompC;
public struct CompD; // intentionally not declared on any archetype

// ── Standard world fixture ────────────────────────────────────────────────────
//
//  Primary:   Requires<CompA>, Allows<CompB>
//  Secondary: Requires<CompB>, Requires<CompC>
//  Tertiary:  Subclass of Primary — inherits CompA (required), CompB (optional),
//             adds CompC (required)

public static class TestWorld
{
    public static TypedWorld<TestEnum> Build() =>
        WorldBuilder.For<TestEnum>()
            .RegisterArchetype(TestEnum.Primary)
                .Requires<CompA>()
                .Allows<CompB>()
            .BuildArchetype()
            .RegisterArchetype(TestEnum.Secondary)
                .Requires<CompB>()
                .Requires<CompC>()
            .BuildArchetype()
            .Subclass(TestEnum.Primary, TestEnum.Tertiary)
                .Requires<CompC>()
            .BuildArchetype()
            .BuildWorld();
}

// ── Test systems ──────────────────────────────────────────────────────────────

public class ValidSystem : TypedSystem
{
    public static readonly QueryDescription Query =
        new QueryDescription().WithAll<CompA, CompB>();

    public override void Run(World world) { }
}

public class UnsatisfiableSystem : TypedSystem
{
    // CompA and CompC together never appear on the same archetype.
    // Primary has CompA but not CompC as required; Secondary has CompC but not CompA.
    // Wait - Tertiary has CompA (inherited) and CompC (required). So this IS satisfiable.
    // Use CompD which appears on no archetype.
    public static readonly QueryDescription Query =
        new QueryDescription().WithAll<CompA, CompD>();

    public override void Run(World world) { }
}

public class ValidEntitySystem : TypedEntitySystem<TestEnum>
{
    public static readonly QueryDescription Query =
        new QueryDescription().WithAll<CompA>();

    public ValidEntitySystem() : base(TestEnum.Primary) { }
    public override void Run(World world) { }
}

public class WrongEnumEntitySystem : TypedEntitySystem<OtherEnum>
{
    public static readonly QueryDescription Query =
        new QueryDescription().WithAll<CompA>();

    public WrongEnumEntitySystem() : base(OtherEnum.X) { }
    public override void Run(World world) { }
}

public class UndeclaredComponentEntitySystem : TypedEntitySystem<TestEnum>
{
    // CompD is not declared on Primary.
    public static readonly QueryDescription Query =
        new QueryDescription().WithAll<CompD>();

    public UndeclaredComponentEntitySystem() : base(TestEnum.Primary) { }
    public override void Run(World world) { }
}

public class CompAOnlySystem : TypedSystem
{
    public static readonly QueryDescription Query =
        new QueryDescription().WithAll<CompA>();

    public override void Run(World world) { }
}

public class ExcludesRequiredEntitySystem : TypedEntitySystem<TestEnum>
{
    // CompA is Required on Primary — excluding it means the query can never match.
    public static readonly QueryDescription Query =
        new QueryDescription().WithAll<CompB>().WithNone<CompA>();

    public ExcludesRequiredEntitySystem() : base(TestEnum.Primary) { }
    public override void Run(World world) { }
}
