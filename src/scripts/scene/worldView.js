import { EventType } from '../../../shared/eventTypes.js';

/**
 * Presentation-side read model of the world. Updated exclusively by applying
 * events emitted from the simulation — never by reading the sim world directly.
 */
export class WorldView {
    size = 0;
    name = '';
    /** @type {{ simTime: number, population: number, demand: { residential: number, commercial: number, industrial: number } }} */
    stats = { simTime: 0, population: 0, demand: { residential: 0, commercial: 0, industrial: 0 } };
    /**
     * 2D array [x][y] of { terrain: string, building: object|null }
     * @type {Array<Array<{terrain: string, building: object|null}>>}
     */
    tiles = [];

    /**
     * @param {object[]} events
     */
    apply(events) {
        for (const event of events) {
            switch (event.type) {
                case EventType.WorldSnapshot:
                    this.#applySnapshot(event);
                    break;
                case EventType.TileChanged:
                    this.#applyTileChanged(event);
                    break;
                case EventType.StatsChanged:
                    this.#applyStatsChanged(event);
                    break;
            }
        }
    }

    /**
     * @param {number} x
     * @param {number} y
     * @returns {{terrain: string, building: object|null}|null}
     */
    getTile(x, y) {
        if (x < 0 || y < 0 || x >= this.size || y >= this.size) return null;
        return this.tiles[x][y];
    }

    /**
     * Returns which of the four cardinal neighbors have a building matching `type`.
     * @param {number} x
     * @param {number} y
     * @param {string} type
     * @returns {{ top: boolean, right: boolean, bottom: boolean, left: boolean }}
     */
    getMatchingNeighbors(x, y, type) {
        return {
            top:    this.getTile(x, y - 1)?.building?.type === type,
            bottom: this.getTile(x, y + 1)?.building?.type === type,
            left:   this.getTile(x - 1, y)?.building?.type === type,
            right:  this.getTile(x + 1, y)?.building?.type === type,
        };
    }

    #applySnapshot(event) {
        this.size = event.size;
        this.name = event.name;
        this.stats = { simTime: event.simTime, population: event.population, demand: { ...event.demand } };
        this.tiles = Array.from({ length: event.size }, () => new Array(event.size).fill(null));
        for (const tile of event.tiles) {
            this.tiles[tile.x][tile.y] = { terrain: tile.terrain, building: tile.building };
        }
    }

    #applyTileChanged(event) {
        if (this.tiles[event.x]) {
            this.tiles[event.x][event.y] = { terrain: event.terrain, building: event.building };
        }
    }

    #applyStatsChanged(event) {
        this.stats = { simTime: event.simTime, population: event.population, demand: { ...event.demand } };
    }
}
