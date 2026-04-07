using Simulation.Model.Buildings.Zones;

namespace Simulation;

using Simulation.Model;
using Systems;
using Systems.Buildings;
using Systems.Services;

public enum SimulationState { Stopped, Running }

/// <summary>
/// Tick-driven city simulator. Emits CQRS-style events to subscribers.
/// Thread-safe: commands and tick are serialised through a single lock.
/// </summary>
public sealed class CitySimulation : IDisposable
{
    private readonly Lock          _lock            = new();
    private readonly BuildingManager _buildingManager = new();
    private readonly List<SimService> _services;
    private readonly List<Action<IReadOnlyList<SimEvent>>> _subscribers = [];

    private Timer?          _timer;
    private SimulationState _state = SimulationState.Stopped;

    private World           _world { get; }

    public CitySimulation(int size, string name = "My City")
    {
        _world    = new World(name, size);
        _services = [new PowerService()];
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

    public void PlaceBuilding(int x, int y, string buildingType)
    {
        lock (_lock)
        {
            var tile = TileTools.GetTile(_world, x, y);
            if (tile is null) return;
            BuildingManager.Create(tile, buildingType);
            if (tile.Updated) Notify([WorldEventFactory.TileChanged(tile)]);
        }
    }

    public void Bulldoze(int x, int y)
    {
        lock (_lock)
        {
            var tile = TileTools.GetTile(_world, x, y);
            if (tile is null) return;
            _buildingManager.Bulldoze(_world, tile);
            if (tile.Updated) Notify([WorldEventFactory.TileChanged(tile)]);
        }
    }

    // ── Tick ──────────────────────────────────────────────────────────────────

    private void Tick()
    {
        lock (_lock)
        {
            CleanWorld();

            foreach (var svc in _services)
                svc.Simulate(_world);

            var vacancies = new List<ResidentialZone>();
            for (var x = 0; x < _world.Size; x++)
            {
                for (var y = 0; y < _world.Size; y++)
                {
                    var tile = _world.Tiles[x][y];
                    _buildingManager.Simulate(_world, tile);
                    if (tile.Building is ResidentialZone { Vacancies: > 0 } rz)
                        vacancies.Add(rz);
                }
            }

            foreach (var zone in vacancies)
            {
                if (Random.Shared.NextDouble() < SimConfig.Modules.Residents.ResidentMoveInChance)
                {
                    var resident = new Citizen { Residence = zone };
                    zone.Residents.Add(resident);
                    _world.Citizens.Add(resident);
                    zone.Updated = zone.Tile.Updated = true;
                }
            }

            var snapshot = _world.Citizens.ToList();
            _world.Citizens.Clear();
            foreach (var c in snapshot.Where(c => CitizenManager.Simulate(_world, c)))
            {
                _world.Citizens.Add(c);
            }

            _world.SimTime++;

            var events = new List<SimEvent>();
            for (var x = 0; x < _world.Size; x++)
                for (var y = 0; y < _world.Size; y++)
                {
                    var tile = _world.Tiles[x][y];
                    if (tile.Updated) events.Add(WorldEventFactory.TileChanged(tile));
                }
            events.Add(WorldEventFactory.StatsChanged(_world));
            Notify(events);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void Notify(IReadOnlyList<SimEvent> events)
    {
        foreach (var sub in _subscribers) sub(events);
    }

    private void CleanWorld()
    {
        foreach (var c in _world.Citizens) c.Updated = false;
        for (int x = 0; x < _world.Size; x++)
            for (int y = 0; y < _world.Size; y++)
            {
                var tile = _world.Tiles[x][y];
                tile.Updated = false;
                tile.Building?.Updated = false;
            }
    }
}
