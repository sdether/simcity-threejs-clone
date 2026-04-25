namespace CitySim.WebService;

using CitySim.Simulation;
using CitySim.Simulation.Systems;

/// <summary>
/// Singleton wrapper around <see cref="CitySimulation"/>.
/// Mirrors node_server/simHost.js: wires the simulation to the WS hub
/// and exposes the command surface used by HTTP endpoints.
/// </summary>
public sealed class SimulationHost : IDisposable
{
    private CitySimulation _sim;
    private Action<IReadOnlyList<SimEvent>>? _subscription;
    private readonly int _worldSize;

    public SimulationHost(int worldSize = 16)
    {
        _worldSize = worldSize;
        _sim       = new CitySimulation(worldSize);
    }

    /// <summary>
    /// Subscribes the hub to simulation events. Must be called once after
    /// both this host and the <see cref="WsHub"/> are available in DI.
    /// </summary>
    public void SetHub(WsHub hub)
    {
        hub.GetSnapshot = GetSnapshot;
        _subscription   = events =>
        {
            foreach (var evt in events)
                hub.Broadcast(evt);
        };
        _sim.Subscribe(_subscription);
    }

    private SimEvent GetSnapshot() => _sim.GetSnapshot();

    // ── Commands ──────────────────────────────────────────────────────────────

    public void PlaceBuilding(int x, int y, string type) => _sim.PlaceBuilding(x, y, type);
    public void Bulldoze(int x, int y)                   => _sim.Bulldoze(x, y);
    public void Pause()                                   => _sim.Halt();
    public void Resume()                                  => _sim.Run();
    public void RequestRefresh()                          => _sim.RequestFullRefresh();

    public void Reset()
    {
        _sim.Dispose();
        _sim = new CitySimulation(_worldSize);
        if (_subscription is not null)
            _sim.Subscribe(_subscription);
        _sim.Run();
    }

    public void Dispose() => _sim.Dispose();
}
