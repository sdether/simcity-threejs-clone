using WebService;
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
app.Map("/ws", async (HttpContext ctx, WsHub hub) =>
{
    if (!ctx.WebSockets.IsWebSocketRequest)
    {
        ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
        return;
    }
    var ws = await ctx.WebSockets.AcceptWebSocketAsync();
    await hub.HandleConnectionAsync(ws, ctx.RequestAborted);
});

// Wolverine HTTP maps all [WolverinePost] endpoints in this assembly.
app.MapWolverineEndpoints();

// ── Wire simulation → hub, then start ─────────────────────────────────────────

var simHost = app.Services.GetRequiredService<SimulationHost>();
var wsHub   = app.Services.GetRequiredService<WsHub>();
simHost.SetHub(wsHub);
simHost.Resume();

var port = app.Configuration["PORT"] ?? "3001";
Console.WriteLine($"CitySim C# server starting on :{port}");
app.Run($"http://0.0.0.0:{port}");
