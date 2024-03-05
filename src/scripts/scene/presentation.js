import * as THREE from 'three';
import {BuildingType} from './buildings/buildingType.js';
import {createBuilding} from './buildings/buildingFactory.js';
import {Tile} from './tile.js';
import {VehicleGraph} from './vehicles/vehicleGraph.js';

export class Presentation extends THREE.Group {
    /**
     * Separate group for organizing debug meshes so they aren't included
     * in raycasting checks
     * @type {THREE.Group}
     */
    debugMeshes = new THREE.Group();
    /**
     * Root node for all scene objects
     * @type {THREE.Group}
     */
    root = new THREE.Group();
    /**
     * The size of the city in tiles
     * @type {number}
     */
    size = 16;
    /**
     * 2D array of tiles that make up the city
     * @type {Tile[][]}
     */
    tiles = [];
    /**
     *
     * @param {VehicleGraph} size
     */
    vehicleGraph;

    constructor(simulation) {
        super();

        this.size = simulation.size;

        this.add(this.debugMeshes);
        this.add(this.root);

        this.tiles = [];
        for (let x = 0; x < this.size; x++) {
            const column = [];
            for (let y = 0; y < this.size; y++) {
                const tile = new Tile(x, y);
                tile.refreshView(simulation);
                this.root.add(tile);
                column.push(tile);
            }
            this.tiles.push(column);
        }

        this.vehicleGraph = new VehicleGraph(this.size);
        this.debugMeshes.add(this.vehicleGraph);
    }

    update(simulation) {
        for (let x = 0; x < this.size; x++) {
            for (let y = 0; y < this.size; y++) {
                let displayTile = this.tiles[x][y]
                let simTile = simulation.getTile(x, y)
                if(simTile.building) {
                    if(!displayTile.building) {
                        displayTile.setBuilding(createBuilding(x, y, simTile.building.type));

                        this.#refreshTileAndNeighbors(displayTile, simulation)

                        if (displayTile.building.type === BuildingType.road) {
                            this.vehicleGraph.updateTile(x, y, tile.building);
                        }
                    } else {
                        displayTile.refreshView(simulation);
                    }
                } else {
                    if(displayTile.building) {
                        if (displayTile.building.type === BuildingType.road) {
                            this.vehicleGraph.updateTile(x, y, null);
                        }

                        displayTile.building.dispose();
                        displayTile.setBuilding(null);
                        this.#refreshTileAndNeighbors(displayTile, simulation)
                    } else {
                        displayTile.refreshView(simulation);
                    }
                }
            }
        }
    }

    #refreshTileAndNeighbors(tile, simulation) {
        tile.refreshView(simulation);
        // Update neighboring tiles in case they need to change their mesh (e.g. roads)
        for (const neighbor of this.getTileNeighbors(tile.x, tile.y)) {
            neighbor.refreshView(simulation)
        }
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

    draw() {
        this.vehicleGraph.updateVehicles();
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