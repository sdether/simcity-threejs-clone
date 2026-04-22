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
            new TypedWorldBuilder()
                .RegisterArcheType<IPrimary>()
                .RegisterArcheType<IPrimary>()
                .Build());
    }

    [Test]
    public void Build_Throws_WhenNonInterfaceRegistered()
    {
        Assert.Throws<ArchetypeValidationException>(() =>
            new TypedWorldBuilder()
                .RegisterArcheType<ConcreteArchetype>()
                .Build());
    }

    [Test]
    public void Archetype_InheritsParentComponents()
    {
        using var world = TestWorld.Build();

        // ITertiary extends IPrimary: inherits CompA (required) + CompB (optional),
        // adds CompC (required).
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

        // ITertiary inherits CompA as required — omitting it should throw.
        var ex = Assert.Throws<ArchetypeValidationException>(() =>
            world.Create<ITertiary>(new CompC()));

        Assert.That(ex!.Message, Does.Contain("CompA"));
    }
}

public class ConcreteArchetype : IArcheType { }
