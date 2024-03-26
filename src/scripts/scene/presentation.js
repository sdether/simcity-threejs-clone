import * as THREE from 'three';
import {createBuilding} from './buildings/buildingFactory.js';
import {DisplayTile, ModelBuilding} from './displayTile.js';
import {World} from "../model/world.js";
import {getTile} from "../sim/tileTools.js";

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
    size;
    /**
     * 2D array of tiles that make up the city
     * @type {DisplayTile[][]}
     */
    tiles = [];

    constructor(world) {
        super();
        this.size = world.size;
        this.add(this.debugMeshes);
        this.add(this.root);

        this.tiles = [];
        let i = 0;
        for (let x = 0; x < this.size; x++) {
            const column = [];
            for (let y = 0; y < this.size; y++) {
                const tile = new DisplayTile(x, y);
                this.root.add(tile);
                column.push(tile);
                i++;
            }
            this.tiles.push(column);
        }
        this.update(world, true);
    }

    /**
     *
     * @param {World} world
     * @param forceUpdate
     */
    update(world, forceUpdate=false) {
        for (let x = 0; x < this.size; x++) {
            for (let y = 0; y < this.size; y++) {
                let displayTile = this.tiles[x][y]
                let simTile = getTile(world, x, y);
                if(!forceUpdate && !simTile.updated) {
                    continue;
                }
                if(simTile.building) {
                    if (!displayTile.building) {
                        displayTile.setBuilding(new ModelBuilding());
                    }
                }
                displayTile.refreshView(world);
            }
        }
    }

    #refreshTileAndNeighbors(tile, world) {
        tile.refreshView(world);
        // Update neighboring tiles in case they need to change their mesh (e.g. roads)
        for (const neighbor of this.#getTileNeighbors(tile.x, tile.y)) {
            neighbor.refreshView(world)
        }
    }

    /** Returns the title at the coordinates. If the coordinates
     * are out of bounds, then `null` is returned.
     * @param {number} x The x-coordinate of the tile
     * @param {number} y The y-coordinate of the tile
     * @returns {DisplayTile | null}
     */
    #getTile(x, y) {
        if (x === undefined || y === undefined ||
            x < 0 || y < 0 ||
            x >= this.size || y >= this.size) {
            return null;
        } else {
            return this.tiles[x][y];
        }
    }

    draw() {
    }

    /**
     * Finds and returns the neighbors of this tile
     * @param {number} x The x-coordinate of the tile
     * @param {number} y The y-coordinate of the tile
     */
    #getTileNeighbors(x, y) {
        const neighbors = [];

        if (x > 0) {
            neighbors.push(this.#getTile(x - 1, y));
        }
        if (x < this.size - 1) {
            neighbors.push(this.#getTile(x + 1, y));
        }
        if (y > 0) {
            neighbors.push(this.#getTile(x, y - 1));
        }
        if (y < this.size - 1) {
            neighbors.push(this.#getTile(x, y + 1));
        }

        return neighbors;
    }
}