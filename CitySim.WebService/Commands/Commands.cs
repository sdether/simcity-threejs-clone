using CitySim.Simulation.Components;

namespace WebService.Commands;

// ── HTTP request bodies ───────────────────────────────────────────────────────

public record PlaceBuildingRequest(int X, int Y, string? Type);
public record BulldozeRequest(int X, int Y);

// ── Wolverine command messages ────────────────────────────────────────────────

public record PlaceBuildingCommand(int X, int Y, BuildingType Type);
public record BulldozeCommand(int X, int Y);
public record PauseCommand;
public record ResumeCommand;
public record RequestRefreshCommand;
public record ResetCommand;
