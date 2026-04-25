namespace CitySim.WebService;

using System.Text.Json;
using System.Text.Json.Serialization;
using CitySim.Simulation.Systems;

/// <summary>
/// Wire-protocol message type strings — mirrors node_server/protocol.js.
/// </summary>
public static class MessageType
{
    public const string WorldSnapshot = "citysimjs/worldSnapshot";
    public const string TileChanged   = "citysimjs/tileChanged";
    public const string StatsChanged  = "citysimjs/statsChanged";

    public static string? ForEventType(string eventType) => eventType switch
    {
        EventType.WorldSnapshot => WorldSnapshot,
        EventType.TileChanged   => TileChanged,
        EventType.StatsChanged  => StatsChanged,
        _                       => null,
    };
}

/// <summary>
/// Builds the envelope JSON payload for a SimEvent.
/// envelope = { messageType, id, correlationId, body }
/// </summary>
public static class EnvelopeSerializer
{
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy        = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition      = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// Serialises <paramref name="evt"/> as a protocol envelope ready to send over the wire.
    /// Returns null for event types that have no corresponding MessageType.
    /// </summary>
    public static byte[]? Serialize(SimEvent evt)
    {
        var msgType = MessageType.ForEventType(evt.Type);
        if (msgType is null) return null;

        // Serialise the concrete event type so all fields are included.
        var body = JsonSerializer.SerializeToNode(evt, evt.GetType(), JsonOptions);

        var envelope = new
        {
            messageType   = msgType,
            id            = Guid.NewGuid(),
            correlationId = (object?)null,
            body,
        };

        return JsonSerializer.SerializeToUtf8Bytes(envelope, JsonOptions);
    }
}
