using TypedArch;

namespace TypedArch.Tests;

[TestFixture]
public class WorldValidatorTests
{
    private TypedWorld<TestEnum> _world = null!;

    [SetUp]    public void SetUp()    => _world = TestWorld.Build();
    [TearDown] public void TearDown() => _world.Dispose();

    // ── General query satisfiability ──────────────────────────────────────────

    [Test]
    public void Validate_NoErrors_WhenSystemQueryIsSatisfiable()
    {
        var report = _world.Validate([new ValidSystem()]);
        Assert.That(report.Errors, Is.Empty, string.Join("\n", report.Errors));
    }

    [Test]
    public void Validate_Error_WhenQueryIsUnsatisfiable()
    {
        var report = _world.Validate([new UnsatisfiableSystem()]);
        Assert.That(report.Errors, Has.Count.GreaterThan(0));
        Assert.That(report.Errors[0], Does.Contain(nameof(UnsatisfiableSystem)));
    }

    // ── Write component validation ────────────────────────────────────────────

    [Test]
    public void Validate_Error_WhenWriteComponentNotInQuery()
    {
        // CompC is not in ValidSystem's query (WithAll<CompA, CompB>).
        var ex = Assert.Throws<ArchetypeValidationException>(() =>
            WorldBuilder.For<TestEnum>()
                .RegisterArchetype(TestEnum.Primary).Requires<CompA>().Allows<CompB>().BuildArchetype()
                .RegisterArchetype(TestEnum.Secondary).Requires<CompB>().Requires<CompC>().BuildArchetype()
                .RegisterArchetype(TestEnum.Tertiary).Requires<CompA>().Requires<CompC>().BuildArchetype()
                .RegisterSystem<ValidSystem>().Writes<CompC>().BuildSystem()
                .BuildWorld());

        Assert.That(ex!.Message, Does.Contain("CompC"));
    }

    // ── TypedEntitySystem: enum mismatch ──────────────────────────────────────

    [Test]
    public void Validate_Error_WhenEntitySystemEnumDoesNotMatchWorld()
    {
        var report = _world.Validate([new WrongEnumEntitySystem()]);
        Assert.That(report.Errors, Has.Count.GreaterThan(0));
        Assert.That(report.Errors[0], Does.Contain(nameof(OtherEnum))
                                    .Or.Contain(nameof(TestEnum)));
    }

    // ── TypedEntitySystem: archetype conformance ──────────────────────────────

    [Test]
    public void Validate_NoErrors_WhenEntitySystemQueryConformsToArchetype()
    {
        var report = _world.Validate([new ValidEntitySystem()]);
        Assert.That(report.Errors, Is.Empty, string.Join("\n", report.Errors));
    }

    [Test]
    public void Validate_Error_WhenEntitySystemQueriesUndeclaredComponent()
    {
        var report = _world.Validate([new UndeclaredComponentEntitySystem()]);
        Assert.That(report.Errors, Has.Count.GreaterThan(0));
        Assert.That(report.Errors[0], Does.Contain("CompD"));
    }

    [Test]
    public void Validate_Error_WhenEntitySystemExcludesRequiredComponent()
    {
        var report = _world.Validate([new ExcludesRequiredEntitySystem()]);
        Assert.That(report.Errors, Has.Count.GreaterThan(0));
        Assert.That(report.Errors[0], Does.Contain("CompA"));
    }

    // ── Dead optional warnings ────────────────────────────────────────────────

    [Test]
    public void Validate_Warns_WhenOptionalComponentNeverQueried()
    {
        // CompAOnlySystem queries only CompA — CompB is optional on Primary/Tertiary
        // but never appears in any query, so it should produce a dead-optional warning.
        var report = _world.Validate([new CompAOnlySystem()]);
        Assert.That(report.Warnings, Has.Some.Contains("CompB"));
    }

    [Test]
    public void Validate_NoDeadOptionalWarning_WhenOptionalIsQueried()
    {
        // ValidSystem queries WithAll<CompA, CompB> — CompB is covered, no dead-optional.
        var report = _world.Validate([new ValidSystem()]);
        Assert.That(report.Warnings, Has.None.Contains("CompB"));
    }
}
