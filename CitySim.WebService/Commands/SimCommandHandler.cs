namespace CitySim.WebService;

using global::WebService.Commands;

/// <summary>
/// Wolverine handler — one Handle overload per command type.
/// Wolverine discovers this by convention (class name ends with "Handler").
/// </summary>
public class SimCommandHandler(SimulationHost host, ILogger<SimCommandHandler> logger)
{
    public void Handle(PlaceBuildingCommand cmd)
    {
        logger.LogDebug("[CMD] placeBuilding  x={X} y={Y} type={Type}", cmd.X, cmd.Y, cmd.Type);
        host.PlaceBuilding(cmd.X, cmd.Y, cmd.Type);
    }

    public void Handle(BulldozeCommand cmd)
    {
        logger.LogDebug("[CMD] bulldoze       x={X} y={Y}", cmd.X, cmd.Y);
        host.Bulldoze(cmd.X, cmd.Y);
    }

    public void Handle(PauseCommand _)
    {
        logger.LogDebug("[CMD] pause");
        host.Pause();
    }

    public void Handle(ResumeCommand _)
    {
        logger.LogDebug("[CMD] resume");
        host.Resume();
    }

    public void Handle(RequestRefreshCommand _)
    {
        logger.LogDebug("[CMD] requestRefresh");
        host.RequestRefresh();
    }

    public void Handle(ResetCommand _)
    {
        logger.LogDebug("[CMD] reset");
        host.Reset();
    }
}
