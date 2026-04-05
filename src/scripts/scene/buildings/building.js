import * as THREE from 'three';
import {DisplayObject} from '../displayObject.js';
import {BuildingStatus} from '../../model/buildings/buildingStatus.js';

export class Building extends DisplayObject {
    /**
     * True if the terrain should not be rendered beneath this building type.
     * @type {boolean}
     */
    hideTerrain = false;
    /**
     * Icon displayed for building status warnings.
     * @type {THREE.Sprite}
     */
    #statusIcon = new THREE.Sprite();

    constructor(x = 0, y = 0) {
        super(x, y);
        this.#statusIcon.visible = false;
        this.#statusIcon.material = new THREE.SpriteMaterial({depthTest: false});
        this.#statusIcon.layers.set(1);
        this.#statusIcon.scale.set(0.5, 0.5, 0.5);
        this.add(this.#statusIcon);
    }

    /**
     * @param {{ terrain: string, building: object }} tileView
     * @param {import('../worldView.js').WorldView} worldView
     */
    refreshView(tileView, worldView) {
        const buildingView = tileView.building;
        switch (buildingView.status) {
            case BuildingStatus.NoPower:
                this.#statusIcon.visible = true;
                this.#statusIcon.material.map = window.assetManager.statusIcons[BuildingStatus.NoPower];
                break;
            case BuildingStatus.NoRoadAccess:
                this.#statusIcon.visible = true;
                this.#statusIcon.material.map = window.assetManager.statusIcons[BuildingStatus.NoRoadAccess];
                break;
            default:
                this.#statusIcon.visible = false;
        }
    }

    /**
     * Renders this building as an uncommitted intent tile — no sim world needed.
     * Subclasses override to show their appropriate initial mesh.
     */
    refreshForIntent() {
        this.#statusIcon.visible = false;
    }

    dispose() {
        super.dispose();
    }
}
