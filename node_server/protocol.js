import { randomUUID } from 'crypto';

export const MessageType = {
  WorldSnapshot: 'citysimjs/worldSnapshot',
  TileChanged:   'citysimjs/tileChanged',
  StatsChanged:  'citysimjs/statsChanged',
};

const eventTypeToMessageType = {
  WorldSnapshot: MessageType.WorldSnapshot,
  TileChanged:   MessageType.TileChanged,
  StatsChanged:  MessageType.StatsChanged,
};

export function makeEnvelope(messageType, body) {
  return { messageType, id: randomUUID(), correlationId: null, body };
}

export function envelopeForEvent(eventBody) {
  const messageType = eventTypeToMessageType[eventBody.type];
  if (!messageType) return null;
  return makeEnvelope(messageType, eventBody);
}
