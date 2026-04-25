using CitySim.WebService;
using WebService.Commands;
using Wolverine;
using Wolverine.Http;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────────────────────────

builder.Services.AddSingleton<SimulationHost>();
builder.Services.AddSingleton<WsHub>();

builder.Services.AddWolverineHttp();

builder.Host.UseWolverine(opts =>
{
    // Wolverine discovers SimCommandHandler by convention.
    opts.Discovery.IncludeAssembly(typeof(SimCommandHandler).Assembly);
});

// ── App ───────────────────────────────────────────────────────────────────────

var app = builder.Build();

app.UseWebSockets();

// WebSocket endpoint — mirrors WsHub.#onConnect in wsHub.js
app.Map("/ws", async (HttpContext ctx, WsHub hub, IHostApplicationLifetime lifetime) =>
{
    if (!ctx.WebSockets.IsWebSocketRequest)
    {
        ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
        return;
    }
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(
        ctx.RequestAborted, lifetime.ApplicationStopping);
    var ws = await ctx.WebSockets.AcceptWebSocketAsync();
    await hub.HandleConnectionAsync(ws, cts.Token);
});

// Wolverine HTTP maps all [WolverinePost] endpoints in this assembly.
app.MapWolverineEndpoints();

// ── Wire simulation → hub, then start ─────────────────────────────────────────

var simHost = app.Services.GetRequiredService<SimulationHost>();
var wsHub   = app.Services.GetRequiredService<WsHub>();
simHost.SetHub(wsHub);
simHost.Resume();

var port      = app.Configuration["PORT"] ?? "3001";
var startLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("CitySim.WebService");
startLogger.LogDebug("CitySim C# server starting on :{Port}", port);
app.Run($"http://0.0.0.0:{port}");
