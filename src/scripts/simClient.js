/**
 * Browser-side transport layer. Replaces direct Simulation calls in game.js.
 * Commands are sent via REST (POST); events arrive via WebSocket.
 * Protocol mimics Wolverine message envelopes for future server swap-out.
 */
export class SimClient {
  #subscriber = null;
  #ws = null;
  #reconnectTimer = null;
  #intentionallyClosed = false;

  /**
   * Open the WebSocket and start receiving events.
   * The subscriber receives an array of event bodies, matching the
   * signature that presentation.applyEvents() already expects.
   * @param {(events: object[]) => void} subscriber
   */
  connect(subscriber) {
    this.#subscriber = subscriber;
    this.#intentionallyClosed = false;
    this.#openSocket();
  }

  disconnect() {
    this.#intentionallyClosed = true;
    clearTimeout(this.#reconnectTimer);
    this.#ws?.close();
  }

  async placeBuilding(x, y, type) {
    await this.#post('placeBuilding', { x, y, type });
  }

  async bulldoze(x, y) {
    await this.#post('bulldoze', { x, y });
  }

  async pause() {
    await this.#post('pause', {});
  }

  async resume() {
    await this.#post('resume', {});
  }

  async requestRefresh() {
    await this.#post('requestRefresh', {});
  }

  async reset() {
    await this.#post('reset', {});
  }

  #openSocket() {
    const wsUrl = `ws://${location.host}/ws`;
    this.#ws = new WebSocket(wsUrl);

    this.#ws.onmessage = (evt) => {
      const envelope = JSON.parse(evt.data);
      // envelope.body is the raw event body the presentation already understands
      this.#subscriber?.([envelope.body]);
    };

    this.#ws.onclose = () => {
      if (!this.#intentionallyClosed) {
        console.warn('[SimClient] WebSocket closed — reconnecting in 2s');
        this.#reconnectTimer = setTimeout(() => this.#openSocket(), 2000);
      }
    };

    this.#ws.onerror = (err) => {
      console.error('[SimClient] WebSocket error', err);
    };
  }

  async #post(command, body) {
    const res = await fetch(`/api/commands/${command}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'wolverine-message-type': `citysimjs/${command}`,
      },
      body: JSON.stringify(body),
    });
    if (!res.ok) {
      console.error(`[SimClient] command ${command} failed: ${res.status}`, await res.text());
    }
  }
}
