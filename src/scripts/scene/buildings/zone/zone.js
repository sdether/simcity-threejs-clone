import * as THREE from 'three';
import { DEG2RAD } from 'three/src/math/MathUtils.js';
import { Building } from '../building.js';
import {DevelopmentState} from '../../../model/buildings/zones/zone.js';

/**
 * Represents a zoned building such as residential, commercial or industrial.
 */
export class Zone extends Building {
    /**
     * The mesh style variant to use when rendering.
     */
    style = ['A', 'B', 'C'][Math.floor(3 * Math.random())];

    constructor(x = 0, y = 0) {
        super(x, y);
        this.rotation.y = 90 * Math.floor(4 * Math.random()) * DEG2RAD;
    }

    refreshForIntent() {
        super.refreshForIntent();
        this.setMesh(window.assetManager.getModel('under-construction', this));
    }

    /**
     * @param {{ terrain: string, building: object }} tileView
     * @param {import('../../worldView.js').WorldView} worldView
     */
    refreshView(tileView, worldView) {
        super.refreshView(tileView, worldView);
        const buildingView = tileView.building;

        let modelName;
        switch (buildingView.development.state) {
            case DevelopmentState.underConstruction:
            case DevelopmentState.undeveloped:
                modelName = 'under-construction';
                break;
            default:
                modelName = `${buildingView.type}-${this.style}${buildingView.development.level}`;
                break;
        }

        const mesh = window.assetManager.getModel(modelName, this);

        if (buildingView.development.state === DevelopmentState.abandoned) {
            mesh.traverse((obj) => {
                if (obj.material) {
                    obj.material.color = new THREE.Color(0x707070);
                }
            });
        }

        this.setMesh(mesh);
    }
}
