using Arch.Core;
using TypedArch;

namespace TypedArch.Tests;

[TestFixture]
public class WorldValidatorTests
{
    private TypedWorld _world = null!;

    [SetUp]    public void SetUp()    => _world = TestWorld.Build();
    [TearDown] public void TearDown() => _world.Dispose();

    // ── General satisfiability ────────────────────────────────────────────────

    [Test]
    public void Validate_NoErrors_WhenQueryIsSatisfiable()
    {
        var report = _world.Validate([new ValidSystem()]);
        Assert.That(report.Errors, Is.Empty, string.Join("\n", report.Errors));
    }

    [Test]
    public void Validate_Error_WhenQueryIsUnsatisfiable()
    {
        var report = _world.Validate([new UnsatisfiableSystem()]);
        Assert.That(report.Errors, Has.Some.Contains(nameof(UnsatisfiableSystem)));
    }

    // ── Typed query archetype conformance ─────────────────────────────────────

    [Test]
    public void Validate_NoErrors_WhenTypedQueryConformsToArchetype()
    {
        var report = _world.Validate([new ValidBoundSystem()]);
        Assert.That(report.Errors, Is.Empty, string.Join("\n", report.Errors));
    }

    [Test]
    public void Validate_Error_WhenTypedQueryUsesUndeclaredComponent()
    {
        var report = _world.Validate([new UndeclaredComponentBoundSystem()]);
        Assert.That(report.Errors, Has.Some.Contains("CompD"));
    }

    [Test]
    public void Validate_Error_WhenTypedQueryExcludesRequiredComponent()
    {
        var report = _world.Validate([new ExcludesRequiredBoundSystem()]);
        Assert.That(report.Errors, Has.Some.Contains("CompA"));
    }

    [Test]
    public void Validate_Error_WhenTypedQueryBoundToUnregisteredArchetype()
    {
        var report = _world.Validate([new UnregisteredArchetypeSystem()]);
        Assert.That(report.Errors, Has.Some.Contains("IUnregistered"));
    }

    // ── Dead optional warnings ────────────────────────────────────────────────

    [Test]
    public void Validate_Warns_WhenOptionalComponentNeverQueried()
    {
        var report = _world.Validate([new CompAOnlySystem()]);
        Assert.That(report.Warnings, Has.Some.Contains("CompB"));
    }

    [Test]
    public void Validate_NoWarning_WhenOptionalIsQueried()
    {
        var report = _world.Validate([new ValidSystem()]);
        Assert.That(report.Warnings, Has.None.Contains("CompB"));
    }
}
