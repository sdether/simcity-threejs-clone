import { WebSocketServer } from 'ws';
import { envelopeForEvent, makeEnvelope, MessageType } from './protocol.js';

function eventSummary(e) {
  switch (e.type) {
    case 'WorldSnapshot': return `WorldSnapshot  size=${e.size} tiles=${e.tiles?.length} pop=${e.population}`;
    case 'TileChanged':   return `TileChanged     x=${e.x} y=${e.y} building=${e.building?.type ?? 'null'} status=${e.building?.status ?? '-'}`;
    case 'StatsChanged':  return `StatsChanged    t=${e.simTime} pop=${e.population} demand=${JSON.stringify(e.demand)}`;
    default:              return e.type;
  }
}

export class WsHub {
  #clients = new Set();
  #getSnapshot;

  constructor(server, getSnapshot) {
    this.#getSnapshot = getSnapshot;
    const wss = new WebSocketServer({ server, path: '/ws' });
    wss.on('connection', (ws) => this.#onConnect(ws));
  }

  #onConnect(ws) {
    this.#clients.add(ws);
    console.log(`[WS]  client connected  (total=${this.#clients.size})`);
    const snapshot = this.#getSnapshot();
    console.log(`[EVT] ${eventSummary(snapshot)}`);
    ws.send(JSON.stringify(makeEnvelope(MessageType.WorldSnapshot, snapshot)));

    ws.on('close', () => {
      this.#clients.delete(ws);
      console.log(`[WS]  client disconnected (total=${this.#clients.size})`);
    });
    ws.on('error', () => this.#clients.delete(ws));
  }

  broadcast(eventBody) {
    const envelope = envelopeForEvent(eventBody);
    if (!envelope) return;
    console.log(`[EVT] ${eventSummary(eventBody)}`);
    const payload = JSON.stringify(envelope);
    for (const ws of this.#clients) {
      if (ws.readyState === 1 /* OPEN */) ws.send(payload);
    }
  }
}
