using TypedArch;

namespace TypedArch.Tests;

[TestFixture]
public class WorldBuilderTests
{
    [Test]
    public void Build_Succeeds_WithRegisteredArchetypes()
    {
        Assert.DoesNotThrow(() => TestWorld.Build().Dispose());
    }

    [Test]
    public void Build_Throws_OnDuplicateArchetype()
    {
        Assert.Throws<ArchetypeValidationException>(() =>
            new TypedWorld<IDuplicateArchetypes, ITestSystems>());
    }

    [Test]
    public void Build_Throws_WhenNonInterfaceRegistered()
    {
        Assert.Throws<ArchetypeValidationException>(() =>
            new TypedWorld<IConcreteArchetypeManifest, ITestSystems>());
    }

    [Test]
    public void Archetype_InheritsParentComponents()
    {
        using var world = TestWorld.Build();

        Assert.DoesNotThrow(() =>
        {
            var entity = world.Create<ITertiary>(new CompA(), new CompB(), new CompC());
            world.Destroy(entity);
        });
    }

    [Test]
    public void Archetype_InheritedRequiredComponent_IsEnforced()
    {
        using var world = TestWorld.Build();

        var ex = Assert.Throws<ArchetypeValidationException>(() =>
            world.Create<ITertiary>(new CompC())); // missing CompA

        Assert.That(ex!.Message, Does.Contain("CompA"));
    }

    [Test]
    public void Create_Succeeds_ForConcreteChildOfPlainBase()
    {
        using var world = new TypedWorld<IConcreteChildManifest, ITestSystems>();

        Assert.DoesNotThrow(() =>
        {
            var entity = world.Create<IConcreteChild>(new CompA(), new CompC());
            world.Destroy(entity);
        });
    }

    [Test]
    public void ConcreteChild_InheritsPlainBaseComponents()
    {
        using var world = new TypedWorld<IConcreteChildManifest, ITestSystems>();

        var ex = Assert.Throws<ArchetypeValidationException>(() =>
            world.Create<IConcreteChild>(new CompC())); // missing CompA from abstract parent

        Assert.That(ex!.Message, Does.Contain("CompA"));
    }
}

// ── Manifests used only in failure-path tests ─────────────────────────────────

public interface IDuplicateArchetypes
{
    IPrimary First  { get; }
    IPrimary Second { get; } // duplicate
}

public interface IConcreteArchetypeManifest
{
    ConcreteArchetype Bad { get; } // not an interface
}

public class ConcreteArchetype : IArcheType { }

public interface IConcreteChildManifest
{
    IConcreteChild ConcreteChild { get; }
}
