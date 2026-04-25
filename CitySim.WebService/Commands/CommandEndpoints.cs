namespace CitySim.WebService;

using CitySim.Simulation.Components;
using Wolverine;
using Wolverine.Http;
using global::WebService.Commands;

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
        var buildingType = req.Type switch
        {
            "residential" => BuildingType.Residential,
            "commercial"  => BuildingType.Commercial,
            "industrial"  => BuildingType.Industrial,
            "road"        => BuildingType.Road,
            "power-plant" => BuildingType.PowerPlant,
            "power-line"  => BuildingType.PowerLine,
            _             => (BuildingType?)null,
        };
        if (buildingType is null)
            return Results.BadRequest(new { error = "x, y, type required" });
        await bus.SendAsync(new PlaceBuildingCommand(req.X, req.Y, buildingType.Value));
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
