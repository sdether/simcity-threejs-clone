using TypedArch;

namespace TypedArch.Tests;

[TestFixture]
public class WorldBuilderTests
{
    [Test]
    public void BuildWorld_Succeeds_WhenAllArchetypesRegistered()
    {
        Assert.DoesNotThrow(() => TestWorld.Build().Dispose());
    }

    [Test]
    public void BuildWorld_Throws_WhenArchetypeMissing()
    {
        var ex = Assert.Throws<ArchetypeValidationException>(() =>
            WorldBuilder.For<TestEnum>()
                .RegisterArchetype(TestEnum.Primary).Requires<CompA>().BuildArchetype()
                // Secondary and Tertiary not registered
                .BuildWorld());

        Assert.That(ex!.Message, Does.Contain("Secondary").Or.Contain("Tertiary"));
    }

    [Test]
    public void BuildWorld_Throws_OnDuplicateArchetype()
    {
        Assert.Throws<ArchetypeValidationException>(() =>
            WorldBuilder.For<TestEnum>()
                .RegisterArchetype(TestEnum.Primary).BuildArchetype()
                .RegisterArchetype(TestEnum.Primary).BuildArchetype() // duplicate
                .RegisterArchetype(TestEnum.Secondary).BuildArchetype()
                .RegisterArchetype(TestEnum.Tertiary).BuildArchetype()
                .BuildWorld());
    }

    [Test]
    public void Subclass_InheritsParentComponents()
    {
        using var world = TestWorld.Build();

        // Tertiary is a subclass of Primary: it should require CompA (inherited) + CompC (own)
        // and allow CompB (inherited optional).
        // Valid creation: CompA + CompC (both required), CompB optional included.
        Assert.DoesNotThrow(() =>
        {
            var entity = world.Create(TestEnum.Tertiary, new CompA(), new CompB(), new CompC());
            world.Destroy(entity);
        });
    }

    [Test]
    public void Subclass_Throws_WhenParentNotRegistered()
    {
        Assert.Throws<ArchetypeValidationException>(() =>
            WorldBuilder.For<TestEnum>()
                .Subclass(TestEnum.Primary, TestEnum.Tertiary) // Primary not yet registered
                .BuildArchetype()
                .RegisterArchetype(TestEnum.Primary).BuildArchetype()
                .RegisterArchetype(TestEnum.Secondary).BuildArchetype()
                .BuildWorld());
    }
}
