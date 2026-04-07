namespace WebService.Commands;

/// <summary>
/// Wolverine handler — one Handle overload per command type.
/// Wolverine discovers this by convention (class name ends with "Handler").
/// </summary>
public class SimCommandHandler(SimulationHost host)
{
    public void Handle(PlaceBuildingCommand cmd)
    {
        Console.WriteLine($"[CMD] placeBuilding  x={cmd.X} y={cmd.Y} type={cmd.Type}");
        host.PlaceBuilding(cmd.X, cmd.Y, cmd.Type);
    }

    public void Handle(BulldozeCommand cmd)
    {
        Console.WriteLine($"[CMD] bulldoze       x={cmd.X} y={cmd.Y}");
        host.Bulldoze(cmd.X, cmd.Y);
    }

    public void Handle(PauseCommand _)
    {
        Console.WriteLine("[CMD] pause");
        host.Pause();
    }

    public void Handle(ResumeCommand _)
    {
        Console.WriteLine("[CMD] resume");
        host.Resume();
    }

    public void Handle(RequestRefreshCommand _)
    {
        Console.WriteLine("[CMD] requestRefresh");
        host.RequestRefresh();
    }

    public void Handle(ResetCommand _)
    {
        Console.WriteLine("[CMD] reset");
        host.Reset();
    }
}
