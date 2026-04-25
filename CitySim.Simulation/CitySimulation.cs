using CitySim.Simulation.Components;
using CitySim.Simulation.Managers;
using CitySim.Simulation.Systems.Citizens;
using Simulation;

namespace CitySim.Simulation;

using Model;
using Systems;
using Systems.Buildings;

public enum SimulationState { Stopped, Running }

/// <summary>
/// Tick-driven city simulator. Emits CQRS-style events to subscribers.
/// Thread-safe: commands and tick are serialised through a single lock.
/// </summary>
public sealed class CitySimulation : IDisposable
{
    private readonly Lock          _lock            = new();
    private readonly BuildingFactory _buildingFactory = new();
    private readonly List<Action<IReadOnlyList<SimEvent>>> _subscribers = [];
    private readonly World _world;
    private readonly SimSystem _powerSystem = new PowerSystem();
    private readonly SimSystem _commerceSystem = new CommerceSystem();
    private readonly SimSystem _developmentSystem = new DevelopmentSystem();
    private readonly SimSystem _jobsSystem = new JobsSystem();
    private readonly SimSystem _residentsSystem = new ResidentsSystem();
    private readonly SimSystem _roadAccessSystem = new RoadAccessSystem();
    private readonly SimSystem _vacancySystem = new VacancySystem();
    private readonly SimSystem _buildingStateSystem = new BuildingStateSystem();
    private readonly SimSystem _homelessnessSystem = new HomelessnessSystem();
    private readonly SimSystem _employmentSystem = new EmploymentSystem();
    
    private Timer?          _timer;
    private SimulationState _state = SimulationState.Stopped;


    public CitySimulation(int size, string name = "My City")
    {
        _world    = new World(name, size);
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    public void Run()
    {
        lock (_lock)
        {
            if (_state == SimulationState.Running) return;
            _timer = new Timer(_ => Tick(), null,
                TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            _state = SimulationState.Running;
        }
    }

    public void Halt()
    {
        lock (_lock)
        {
            if (_state == SimulationState.Stopped) return;
            _timer?.Dispose();
            _timer = null;
            _state = SimulationState.Stopped;
        }
    }

    public void Dispose() => Halt();

    // ── Pub/Sub ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Registers a subscriber and immediately fires a WorldSnapshot so the
    /// subscriber can initialize without querying the world directly.
    /// </summary>
    public void Subscribe(Action<IReadOnlyList<SimEvent>> subscriber)
    {
        lock (_lock)
        {
            _subscribers.Add(subscriber);
            subscriber([WorldEventFactory.WorldSnapshot(_world)]);
        }
    }

    public WorldSnapshotEvent GetSnapshot()
    {
        lock (_lock) return WorldEventFactory.WorldSnapshot(_world);
    }

    public void RequestFullRefresh()
    {
        lock (_lock) Notify([WorldEventFactory.WorldSnapshot(_world)]);
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    public void PlaceBuilding(int x, int y, BuildingType buildingType)
    {
        lock (_lock)
        {
            if (_world.GetGridPosition(x, y) is { } position)
            {
                var entity = _world.GetTile(position);
                if (entity.HasValue) return;
                BuildingFactory.Create(_world, position, buildingType);
            }
        }
    }

    public void Bulldoze(int x, int y)
    {
        lock (_lock)
        {
            if (_world.GetGridPosition(x, y) is { } position)
            {
                var entity = _world.GetTile(position);
                if (!entity.HasValue) return;
                _buildingFactory.Bulldoze(_world, position);
            }
        }
    }

    // ── Tick ──────────────────────────────────────────────────────────────────

    private void Tick()
    {
        lock (_lock)
        { 
            _powerSystem.Run(_world);
            _roadAccessSystem.Run(_world);
            _developmentSystem.Run(_world);
            _jobsSystem.Run(_world);
            _residentsSystem.Run(_world);
            _commerceSystem.Run(_world);
            _vacancySystem.Run(_world);
            _buildingStateSystem.Run(_world);
            _homelessnessSystem.Run(_world);
            _employmentSystem.Run(_world);

            _world.SimTime++;

            var events = _world.ResetChangedTiles()
                .Select(tile => WorldEventFactory.TileChanged(_world, tile))
                .Cast<SimEvent>()
                .ToList();

            events.Add(WorldEventFactory.StatsChanged(_world));
            Notify(events);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void Notify(IReadOnlyList<SimEvent> events)
    {
        foreach (var sub in _subscribers) sub(events);
    }
}
