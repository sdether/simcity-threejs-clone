using Simulation.Model;
using TypedArch;

namespace Simulation.Tests;

/// <summary>
/// Validates that the simulation world's archetype registration is correct.
/// These tests act as the post-build validation gate.
/// </summary>
[TestFixture]
public class WorldArchetypeTests
{
    private World _world = null!;

    [SetUp]    public void SetUp()    => _world = new World("test", 4);
    [TearDown] public void TearDown() => _world.Ecs.Dispose();

    [Test]
    public void World_CanBeConstructed()
    {
        // Verifies the WorldBuilder chain in World.BuildWorld() throws no exceptions.
        Assert.That(_world, Is.Not.Null);
    }

    [Test]
    public void World_Validate_HasNoErrors()
    {
        // Registers all known systems and checks there are no archetype violations.
        // Expand RegisterSystem calls here as systems are migrated to TypedSystem.
        var report = _world.Ecs.Validate();
        Assert.That(report.Errors, Is.Empty, string.Join("\n", report.Errors));
    }

    [Test]
    public void World_Tiles_AreCreatedWithCorrectArchetype()
    {
        for (var x = 0; x < _world.Size; x++)
        for (var y = 0; y < _world.Size; y++)
        {
            var tile = _world.Tiles[x][y];
            Assert.That(_world.Ecs.GetArchetype(tile), Is.EqualTo(ArchetypeId.Tile),
                $"Tile at ({x},{y}) has wrong archetype.");
        }
    }
}
