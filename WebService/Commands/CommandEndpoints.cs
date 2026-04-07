namespace WebService.Commands;

using Wolverine;
using Wolverine.Http;

/// <summary>
/// HTTP endpoints for simulation commands — mirrors node_server/routes/commands.js.
/// Each endpoint validates the request body, then cascades the corresponding
/// Wolverine command message for the handler to execute.
/// </summary>
public static class CommandEndpoints
{
    [WolverinePost("/api/commands/placeBuilding")]
    public static async Task<IResult> PlaceBuilding(
        PlaceBuildingRequest req, IMessageBus bus)
    {
        if (string.IsNullOrWhiteSpace(req.Type))
            return Results.BadRequest(new { error = "x, y, type required" });
        await bus.SendAsync(new PlaceBuildingCommand(req.X, req.Y, req.Type));
        return Results.Accepted();
    }

    [WolverinePost("/api/commands/bulldoze")]
    public static async Task<IResult> Bulldoze(
        BulldozeRequest req, IMessageBus bus)
    {
        await bus.SendAsync(new BulldozeCommand(req.X, req.Y));
        return Results.Accepted();
    }

    [WolverinePost("/api/commands/pause")]
    public static async Task<IResult> Pause(IMessageBus bus)
    {
        await bus.SendAsync(new PauseCommand());
        return Results.Accepted();
    }

    [WolverinePost("/api/commands/resume")]
    public static async Task<IResult> Resume(IMessageBus bus)
    {
        await bus.SendAsync(new ResumeCommand());
        return Results.Accepted();
    }

    [WolverinePost("/api/commands/requestRefresh")]
    public static async Task<IResult> RequestRefresh(IMessageBus bus)
    {
        await bus.SendAsync(new RequestRefreshCommand());
        return Results.Accepted();
    }

    [WolverinePost("/api/commands/reset")]
    public static async Task<IResult> Reset(IMessageBus bus)
    {
        await bus.SendAsync(new ResetCommand());
        return Results.Accepted();
    }
}
