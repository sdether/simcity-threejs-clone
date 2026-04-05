import * as THREE from 'three';
import {Building} from './buildings/building.js';
import {DisplayObject} from './displayObject.js';
import {WorldView} from './worldView.js';

export class DisplayTile extends DisplayObject {
    /**
     * The type of terrain
     * @type {string}
     */
    terrain = 'grass';
    /**
     * The building on this tile
     * @type {Building?}
     */
    #building = null;

    constructor(x, y) {
        super(x, y);
    }

    /**
     * @type {Building}
     */
    get building() {
        return this.#building;
    }

    setBuilding(value) {
        if (this.#building) {
            this.#building.dispose();
            this.remove(this.#building);
        }
        this.#building = value;
        if (value) {
            this.add(this.#building);
        }
    }

    /**
     * @param {{ terrain: string, building: object|null }} tileView
     * @param {WorldView} worldView
     */
    refreshView(tileView, worldView) {
        this.terrain = tileView.terrain;
        this.building?.refreshView(tileView, worldView);
        if (this.building?.hideTerrain) {
            this.setMesh(null);
        } else {
            const mesh = window.assetManager.getModel(this.terrain, this);
            mesh.name = this.terrain;
            this.setMesh(mesh);
        }
    }
}
