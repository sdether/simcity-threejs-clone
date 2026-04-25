namespace WebService;

using System.Collections.Concurrent;
using System.Net.WebSockets;
using Simulation.Systems;

/// <summary>
/// Manages all active WebSocket connections and broadcasts simulation events.
/// Mirrors node_server/wsHub.js.
/// </summary>
public sealed class WsHub
{
    private readonly ConcurrentDictionary<string, (WebSocket Socket, SemaphoreSlim Lock)> _clients = new();

    /// <summary>Set by Program.cs after both SimulationHost and WsHub are created.</summary>
    public Func<SimEvent>? GetSnapshot { get; set; }

    /// <summary>
    /// Accepts a WebSocket connection, sends the current world snapshot, then
    /// keeps the socket alive until the client disconnects.
    /// </summary>
    public async Task HandleConnectionAsync(WebSocket ws, CancellationToken ct)
    {
        var id      = Guid.NewGuid().ToString();
        var sendLock = new SemaphoreSlim(1, 1);
        _clients[id] = (ws, sendLock);
        Console.WriteLine($"[WS]  client connected  (total={_clients.Count})");

        // Send snapshot immediately on connect.
        if (GetSnapshot?.Invoke() is { } snapshot)
        {
            Console.WriteLine($"[EVT] {Summary(snapshot)}");
            if (EnvelopeSerializer.Serialize(snapshot) is { } payload)
                await SendAsync(ws, sendLock, payload, ct);
        }

        // Drain incoming frames (we don't use them, but we must read to detect close).
        var buffer = new byte[256];
        try
        {
            while (!ct.IsCancellationRequested && ws.State == WebSocketState.Open)
            {
                var result = await ws.ReceiveAsync(buffer, ct);
                if (result.MessageType == WebSocketMessageType.Close) break;
            }
        }
        catch (OperationCanceledException) { }
        catch (WebSocketException) { }
        finally
        {
            _clients.TryRemove(id, out _);
            Console.WriteLine($"[WS]  client disconnected (total={_clients.Count})");
            if (ws.State == WebSocketState.Open)
            {
                try { await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None); }
                catch { /* ignore close errors */ }
            }
        }
    }

    /// <summary>Broadcasts one event to all connected clients.</summary>
    public void Broadcast(SimEvent evt)
    {
        var payload = EnvelopeSerializer.Serialize(evt);
        if (payload is null) return;

        Console.WriteLine($"[EVT] {Summary(evt)}");
        foreach (var (_, (socket, sendLock)) in _clients)
        {
            if (socket.State != WebSocketState.Open) continue;
            // Fire-and-forget per client; exceptions are swallowed to keep others alive.
            _ = SendAsync(socket, sendLock, payload, CancellationToken.None)
                  .ContinueWith(t =>
                      Console.WriteLine($"[WS]  send error: {t.Exception?.GetBaseException().Message}"),
                      TaskContinuationOptions.OnlyOnFaulted);
        }
    }

    private static async Task SendAsync(WebSocket ws, SemaphoreSlim gate, byte[] payload, CancellationToken ct)
    {
        await gate.WaitAsync(ct);
        try { await ws.SendAsync(payload, WebSocketMessageType.Text, endOfMessage: true, ct); }
        finally { gate.Release(); }
    }

    private static string Summary(SimEvent e) => e switch
    {
        WorldSnapshotEvent s => $"WorldSnapshot  size={s.Size} tiles={s.Tiles.Count} pop={s.Stats.Population}",
        TileChangedEvent   t => $"TileChanged     x={t.Tile.X} y={t.Tile.Y} building={t.Tile.Building?.Type ?? "null"} status={t.Tile.Building?.Status ?? "-"}",
        StatsChangedEvent  s => $"StatsChanged    t={s.Stats.SimTime} pop={s.Stats.Population}",
        _                    => e.Type,
    };
}
