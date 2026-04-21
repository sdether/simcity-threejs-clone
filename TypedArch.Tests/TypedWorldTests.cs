using TypedArch;

namespace TypedArch.Tests;

[TestFixture]
public class TypedWorldTests
{
    private TypedWorld<TestEnum> _world = null!;

    [SetUp]    public void SetUp()    => _world = TestWorld.Build();
    [TearDown] public void TearDown() => _world.Dispose();

    // ── Create ────────────────────────────────────────────────────────────────

    [Test]
    public void Create_Succeeds_WithExactRequiredComponents()
    {
        Assert.DoesNotThrow(() =>
        {
            var entity = _world.Create(TestEnum.Primary, new CompA());
            _world.Destroy(entity);
        });
    }

    [Test]
    public void Create_Succeeds_WithRequiredAndOptionalComponents()
    {
        Assert.DoesNotThrow(() =>
        {
            var entity = _world.Create(TestEnum.Primary, new CompA(), new CompB());
            _world.Destroy(entity);
        });
    }

    [Test]
    public void Create_Throws_WhenRequiredComponentMissing()
    {
        // Primary requires CompA — omitting it should throw.
        var ex = Assert.Throws<ArchetypeValidationException>(() =>
            _world.Create(TestEnum.Primary /* no CompA */));

        Assert.That(ex!.Message, Does.Contain("CompA"));
    }

    [Test]
    public void Create_Throws_WhenUndeclaredComponentProvided()
    {
        // CompD is not declared on Primary.
        var ex = Assert.Throws<ArchetypeValidationException>(() =>
            _world.Create(TestEnum.Primary, new CompA(), new CompD()));

        Assert.That(ex!.Message, Does.Contain("CompD"));
    }

    // ── Add ───────────────────────────────────────────────────────────────────

    [Test]
    public void Add_Succeeds_ForOptionalComponent()
    {
        var entity = _world.Create(TestEnum.Primary, new CompA());
        Assert.DoesNotThrow(() => _world.Add(entity, new CompB()));
        _world.Destroy(entity);
    }

    [Test]
    public void Add_Throws_WhenComponentNotInArchetype()
    {
        var entity = _world.Create(TestEnum.Primary, new CompA());

        var ex = Assert.Throws<ArchetypeValidationException>(() =>
            _world.Add(entity, new CompD()));

        Assert.That(ex!.Message, Does.Contain("CompD"));
        _world.Destroy(entity);
    }

    // ── Remove ────────────────────────────────────────────────────────────────

    [Test]
    public void Remove_Succeeds_ForOptionalComponent()
    {
        var entity = _world.Create(TestEnum.Primary, new CompA(), new CompB());
        Assert.DoesNotThrow(() => _world.Remove<CompB>(entity));
        _world.Destroy(entity);
    }

    [Test]
    public void Remove_Throws_WhenComponentIsRequired()
    {
        var entity = _world.Create(TestEnum.Primary, new CompA());

        var ex = Assert.Throws<ArchetypeValidationException>(() =>
            _world.Remove<CompA>(entity));

        Assert.That(ex!.Message, Does.Contain("CompA"));
        _world.Destroy(entity);
    }

    // ── GetArchetype ──────────────────────────────────────────────────────────

    [Test]
    public void GetArchetype_ReturnsCorrectEnum()
    {
        var entity = _world.Create(TestEnum.Secondary, new CompB(), new CompC());
        Assert.That(_world.GetArchetype(entity), Is.EqualTo(TestEnum.Secondary));
        _world.Destroy(entity);
    }
}
