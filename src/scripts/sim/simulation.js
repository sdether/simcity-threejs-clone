import {BuildingType} from './buildings/buildingType.js';
import {createBuilding} from './buildings/buildingFactory.js';
import {Tile} from './tile.js';
import {PowerService} from './services/power.js';
import {SimService} from './services/simService.js';

export const SimulationState = {
    Stopped: 'stopped',
    Running: 'running',
}

export class Simulation {

    state = SimulationState.Stopped;
    /**
     * List of services for the city
     * @type {SimService[]}
     */
    intervalId = 0;

    services = [];
    /**
     * The size of the city in tiles
     * @type {number}
     */
    size = 16;
    /**
     * The current simulation time
     */
    simTime = 0;
    /**
     * 2D array of tiles that make up the city
     * @type {Tile[][]}
     */
    tiles = [];

    subscribers = [];

    constructor(size, name = 'My City') {

        this.name = name;
        this.size = size;
        this.tiles = [];
        for (let x = 0; x < this.size; x++) {
            const column = [];
            for (let y = 0; y < this.size; y++) {
                const tile = new Tile(x, y);
                column.push(tile);
            }
            this.tiles.push(column);
        }

        this.services = [];
        this.services.push(new PowerService());

    }

    /**
     * The total population of the city
     * @type {number}
     */
    get population() {
        let population = 0;
        for (let x = 0; x < this.size; x++) {
            for (let y = 0; y < this.size; y++) {
                const tile = this.getTile(x, y);
                population += tile.building?.residents?.count ?? 0;
            }
        }
        return population;
    }

    /** Returns the title at the coordinates. If the coordinates
     * are out of bounds, then `null` is returned.
     * @param {number} x The x-coordinate of the tile
     * @param {number} y The y-coordinate of the tile
     * @returns {Tile | null}
     */
    getTile(x, y) {
        if (x === undefined || y === undefined ||
            x < 0 || y < 0 ||
            x >= this.size || y >= this.size) {
            return null;
        } else {
            return this.tiles[x][y];
        }
    }

    start() {
        if (this.state === SimulationState.Running) return;
        self.intervalId = setInterval(this.tick.bind(this), 1000);
    }

    stop() {
        if (this.state === SimulationState.Stopped) return;
        clearInterval(self.intervalId);
    }

    subscribe(subscriber) {
        this.subscribers.push(subscriber);
    }

    notifySubscribers() {
        for (const subscriber of this.subscribers) {
            subscriber();
        }
    }

    tick() {
        // Update services
        this.services.forEach((service) => service.simulate(this));

        // Update each building
        for (let x = 0; x < this.size; x++) {
            for (let y = 0; y < this.size; y++) {
                this.getTile(x, y).simulate(this);
            }
        }
        this.simTime++;
        this.notifySubscribers();
    }


    /**
     * Places a building at the specified coordinates if the
     * tile does not already have a building on it
     * @param {number} x
     * @param {number} y
     * @param {string} buildingType
     */
    placeBuilding(x, y, buildingType) {
        const tile = this.getTile(x, y);

        // If the tile doesnt' already have a building, place one there
        if (tile && !tile.building) {
            tile.setBuilding(createBuilding(x, y, buildingType));
        }
        this.notifySubscribers();
    }

    /**
     * Bulldozes the building at the specified coordinates
     * @param {number} x
     * @param {number} y
     */
    bulldoze(x, y) {
        const tile = this.getTile(x, y);

        if (tile.building) {

            tile.building.dispose();
            tile.setBuilding(null);
        }
        this.notifySubscribers();
    }

    /**
     * Finds the first tile where the criteria are true
     * @param {{x: number, y: number}} start The starting coordinates of the search
     * @param {(Tile) => (boolean)} filter This function is called on each
     * tile in the search field until `filter` returns true, or there are
     * no more tiles left to search.
     * @param {number} maxDistance The maximum distance to search from the starting tile
     * @returns {Tile | null} The first tile matching `criteria`, otherwiser `null`
     */
    findTile(start, filter, maxDistance) {
        const startTile = this.getTile(start.x, start.y);
        const visited = new Set();
        const tilesToSearch = [];

        // Initialze our search with the starting tile
        tilesToSearch.push(startTile);

        while (tilesToSearch.length > 0) {
            const tile = tilesToSearch.shift();

            // Has this tile been visited? If so, ignore it and move on
            if (visited.has(tile.id)) {
                continue;
            } else {
                visited.add(tile.id);
            }

            // Check if tile is outside the search bounds
            const distance = startTile.distanceTo(tile);
            if (distance > maxDistance) continue;

            // Add this tiles neighbor's to the search list
            tilesToSearch.push(...this.getTileNeighbors(tile.x, tile.y));

            // If this tile passes the criteria
            if (filter(tile)) {
                return tile;
            }
        }

        return null;
    }

    /**
     * Finds and returns the neighbors of this tile
     * @param {number} x The x-coordinate of the tile
     * @param {number} y The y-coordinate of the tile
     */
    getTileNeighbors(x, y) {
        const neighbors = [];

        if (x > 0) {
            neighbors.push(this.getTile(x - 1, y));
        }
        if (x < this.size - 1) {
            neighbors.push(this.getTile(x + 1, y));
        }
        if (y > 0) {
            neighbors.push(this.getTile(x, y - 1));
        }
        if (y < this.size - 1) {
            neighbors.push(this.getTile(x, y + 1));
        }

        return neighbors;
    }
}