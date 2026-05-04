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
