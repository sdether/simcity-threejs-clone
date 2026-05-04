using TypedArch;

namespace TypedArch.Tests;

[TestFixture]
public class ArchetypeDefinitionTests
{
    // ── Component extraction ──────────────────────────────────────────────────

    [Test]
    public void Required_ContainsDeclaredNonNullableProperties()
    {
        var def = ArchetypeDefinition.ExtractFrom(typeof(IPrimary));
        Assert.That(def.Required, Contains.Item(typeof(CompA)));
    }

    [Test]
    public void Optional_ContainsDeclaredNullableProperties()
    {
        var def = ArchetypeDefinition.ExtractFrom(typeof(IPrimary));
        Assert.That(def.Optional, Contains.Item(typeof(CompB)));
        Assert.That(def.Required, Does.Not.Contain(typeof(CompB)));
    }

    [Test]
    public void Required_IncludesInheritedComponents()
    {
        var def = ArchetypeDefinition.ExtractFrom(typeof(ITertiary));
        Assert.That(def.Required, Contains.Item(typeof(CompA))); // inherited from IPrimary
        Assert.That(def.Required, Contains.Item(typeof(CompC))); // declared on ITertiary
    }

    [Test]
    public void Optional_IncludesInheritedOptionalComponents()
    {
        var def = ArchetypeDefinition.ExtractFrom(typeof(ITertiary));
        Assert.That(def.Optional, Contains.Item(typeof(CompB))); // inherited optional from IPrimary
    }

    [Test]
    public void Required_IncludesComponentsFromPlainBaseInterface()
    {
        var def = ArchetypeDefinition.ExtractFrom(typeof(IConcreteChild));
        Assert.That(def.Required, Contains.Item(typeof(CompA))); // declared on IAbstractBase
        Assert.That(def.Required, Contains.Item(typeof(CompC))); // declared on IConcreteChild
    }

    // ── TypedQueryDescription.From<T> ─────────────────────────────────────────

    [Test]
    public void From_PrePopulatesAllRequiredComponents()
    {
        var tqd   = TypedQueryDescription.For<IPrimary>();
        var span  = tqd.Inner.All.Components.ToArray();
        var types = span.Select(ct => ct.Type).ToHashSet();
        Assert.That(types, Contains.Item(typeof(CompA)));
    }

    [Test]
    public void From_DoesNotIncludeOptionalComponents()
    {
        var tqd   = TypedQueryDescription.For<IPrimary>();
        var span  = tqd.Inner.All.Components.ToArray();
        var types = span.Select(ct => ct.Type).ToHashSet();
        Assert.That(types, Does.Not.Contain(typeof(CompB)));
    }
}
