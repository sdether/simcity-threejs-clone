import { Simulation } from '../src/scripts/sim/simulation.js';
import { worldSnapshotEvent } from '../src/scripts/sim/worldEvents.js';

export class SimHost {
  #sim;
  #hub = null;

  constructor(size = 16) {
    this.#sim = new Simulation(size);
    this.#sim.subscribe((events) => {
      if (!this.#hub) return;
      for (const event of events) {
        this.#hub.broadcast(event);
      }
    });
  }

  setHub(hub) {
    this.#hub = hub;
  }

  getSnapshot() {
    return worldSnapshotEvent(this.#sim.world);
  }

  placeBuilding(x, y, type) { this.#sim.placeBuilding(x, y, type); }
  bulldoze(x, y)             { this.#sim.bulldoze(x, y); }
  pause()                    { this.#sim.halt(); }
  resume()                   { this.#sim.run(); }
  requestRefresh()           { this.#sim.requestFullRefresh(); }

  reset() {
    const size = this.#sim.world.size;
    this.#sim.halt();
    this.#sim = new Simulation(size);
    this.#sim.subscribe((events) => {
      if (!this.#hub) return;
      for (const event of events) {
        this.#hub.broadcast(event);
      }
    });
    this.#sim.run();
  }
}
