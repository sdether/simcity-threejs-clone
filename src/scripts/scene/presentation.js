import * as THREE from 'three';
import {createBuilding} from './buildings/buildingFactory.js';
import {DisplayTile} from './displayTile.js';
import {WorldView} from './worldView.js';
import {EventType} from '../../../shared/eventTypes.js';
import {BuildingType} from '../../../shared/buildingTypes.js';

export class Presentation extends THREE.Group {
    /**
     * Separate group for debug meshes so they are excluded from raycasting.
     * @type {THREE.Group}
     */
    debugMeshes = new THREE.Group();
    /**
     * Root node for all scene objects.
     * @type {THREE.Group}
     */
    root = new THREE.Group();
    /**
     * The city size in tiles. Set when the first WorldSnapshot is applied.
     * @type {number}
     */
    size = 0;
    /**
     * 2D array of display tiles [x][y].
     * @type {DisplayTile[][]}
     */
    tiles = [];
    /**
     * Presentation-side read model — never references the sim world.
     * @type {WorldView}
     */
    worldView = new WorldView();

    constructor() {
        super();
        this.add(this.debugMeshes);
        this.add(this.root);
    }

    /**
     * Apply a batch of simulation events. Drives all visual updates.
     * @param {object[]} events
     */
    applyEvents(events) {
        const tilesToRefresh = new Set();
        const neighborsToRefresh = new Set();

        for (const event of events) {
            console.log('[EVENT]', event);
            if (event.type === EventType.WorldSnapshot) {
                this.worldView.apply([event]);
                this.#initFromSnapshot();
                return; // full reinit — nothing else to process
            } else if (event.type === EventType.TileChanged) {
                const { x, y } = event.tile;
                const hadBuilding = !!this.worldView.getTile(x, y)?.building;
                this.worldView.apply([event]);
                const hasBuilding = !!this.worldView.getTile(x, y)?.building;

                tilesToRefresh.add(`${x},${y}`);

                // Structural change: building added or removed — neighbors need visual refresh
                // (e.g. roads and power lines update their connectivity style)
                if (hadBuilding !== hasBuilding) {
                    for (const key of this.#neighborKeys(x, y)) {
                        neighborsToRefresh.add(key);
                    }
                }
            } else if (event.type === EventType.StatsChanged) {
                this.worldView.apply([event]);
            }
        }

        for (const key of new Set([...tilesToRefresh, ...neighborsToRefresh])) {
            const [x, y] = key.split(',').map(Number);
            this.#refreshDisplayTile(x, y);
        }
    }

    /**
     * Speculatively renders a building on a tile without sending any simulation
     * command. Called during the intent phase (mouse held). The simulation
     * event on mouse-up commit will confirm and replace these visuals.
     * @param {number} x
     * @param {number} y
     * @param {string} type BuildingType
     */
    showIntent(x, y, type) {
        const displayTile = this.#getDisplayTile(x, y);
        if (!displayTile || displayTile.building) return;

        displayTile.setBuilding(createBuilding(x, y, type));
        displayTile.building.setIntent(true);

        if (type === BuildingType.road || type === BuildingType.powerLine) {
            this.#refreshDisplayNeighborAware(displayTile);
            for (const neighbor of this.#getTileNeighbors(x, y)) {
                const ntype = neighbor?.building?.buildingType;
                if (ntype === BuildingType.road || ntype === BuildingType.powerLine) {
                    this.#refreshDisplayNeighborAware(neighbor);
                }
            }
        } else {
            displayTile.building.refreshForIntent();
            if (!displayTile.building.hideTerrain) {
                const mesh = window.assetManager.getModel(displayTile.terrain, displayTile);
                mesh.name = displayTile.terrain;
                displayTile.setMesh(mesh);
            }
        }
    }

    draw() {}

    // ---- private ----

    #initFromSnapshot() {
        this.size = this.worldView.size;
        this.root.clear();
        this.tiles = [];

        for (let x = 0; x < this.size; x++) {
            const column = [];
            for (let y = 0; y < this.size; y++) {
                const tile = new DisplayTile(x, y);
                this.root.add(tile);
                column.push(tile);
            }
            this.tiles.push(column);
        }

        for (let x = 0; x < this.size; x++) {
            for (let y = 0; y < this.size; y++) {
                this.#refreshDisplayTile(x, y);
            }
        }
    }

    #refreshDisplayTile(x, y) {
        const displayTile = this.#getDisplayTile(x, y);
        const tileView = this.worldView.getTile(x, y);
        if (!displayTile || !tileView) return;

        if (tileView.building) {
            if (!displayTile.building) {
                displayTile.setBuilding(createBuilding(x, y, tileView.building.type));
            }
        } else {
            if (displayTile.building) {
                displayTile.setBuilding(null);
            }
        }

        displayTile.refreshView(tileView, this.worldView);
    }

    /**
     * Refreshes a connectivity-dependent building (road, power line) using the
     * current display tile grid instead of the sim world, so intent tiles are
     * included in the neighbor check.
     * @param {DisplayTile} displayTile
     */
    #refreshDisplayNeighborAware(displayTile) {
        const { x, y } = displayTile;
        const btype = displayTile.building?.buildingType;
        if (!btype) return;

        const top    = this.#getDisplayTile(x, y - 1)?.building?.buildingType === btype;
        const bottom = this.#getDisplayTile(x, y + 1)?.building?.buildingType === btype;
        const left   = this.#getDisplayTile(x - 1, y)?.building?.buildingType === btype;
        const right  = this.#getDisplayTile(x + 1, y)?.building?.buildingType === btype;

        displayTile.building.refreshFromNeighbors(top, right, bottom, left);

        if (displayTile.building.hideTerrain) {
            displayTile.setMesh(null);
        } else {
            const mesh = window.assetManager.getModel(displayTile.terrain, displayTile);
            mesh.name = displayTile.terrain;
            displayTile.setMesh(mesh);
        }
    }

    #getDisplayTile(x, y) {
        if (x < 0 || y < 0 || x >= this.size || y >= this.size) return null;
        return this.tiles[x][y];
    }

    #getTileNeighbors(x, y) {
        const neighbors = [];
        if (x > 0)              neighbors.push(this.#getDisplayTile(x - 1, y));
        if (x < this.size - 1)  neighbors.push(this.#getDisplayTile(x + 1, y));
        if (y > 0)              neighbors.push(this.#getDisplayTile(x, y - 1));
        if (y < this.size - 1)  neighbors.push(this.#getDisplayTile(x, y + 1));
        return neighbors;
    }

    #neighborKeys(x, y) {
        const keys = [];
        if (x > 0)              keys.push(`${x - 1},${y}`);
        if (x < this.size - 1)  keys.push(`${x + 1},${y}`);
        if (y > 0)              keys.push(`${x},${y - 1}`);
        if (y < this.size - 1)  keys.push(`${x},${y + 1}`);
        return keys;
    }
}
