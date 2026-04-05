import * as THREE from 'three';
import {Building} from '../building.js';

const Side = {
    Left: 'left',
    Right: 'right',
    Top: 'top',
    Bottom: 'bottom',
};

const powerLineMaterial = new THREE.LineBasicMaterial({color: 0});

export class PowerLine extends Building {

    /**
     * @param {{ terrain: string, building: object }} tileView
     * @param {import('../../worldView.js').WorldView} worldView
     */
    refreshView(tileView, worldView) {
        const {top, right, bottom, left} = worldView.getMatchingNeighbors(this.x, this.y, tileView.building.type);
        this.refreshFromNeighbors(top, right, bottom, left);
    }

    /**
     * Updates the power line mesh given pre-computed neighbor connectivity.
     * Used by both the committed path (via refreshView) and the intent path.
     * @param {boolean} top
     * @param {boolean} right
     * @param {boolean} bottom
     * @param {boolean} left
     */
    refreshFromNeighbors(top, right, bottom, left) {
        const group = new THREE.Group();
        const tower = window.assetManager.getModel(this.buildingType ?? 'power-line', this);
        tower.rotation.y = Math.PI / 4;
        group.add(tower);

        if (top)    this.#addLines(group, Side.Top);
        if (bottom) this.#addLines(group, Side.Bottom);
        if (left)   this.#addLines(group, Side.Left);
        if (right)  this.#addLines(group, Side.Right);

        this.setMesh(group);
    }

    #addLines(group, side) {
        switch (side) {
            case Side.Left:
                group.add(this.#createPowerLine(-0.09, 0.36,  0.09, -0.5, 0.36,  0.09));
                group.add(this.#createPowerLine(-0.09, 0.36, -0.09, -0.5, 0.36, -0.09));
                break;
            case Side.Right:
                group.add(this.#createPowerLine(0.09, 0.36,  0.09,  0.5, 0.36,  0.09));
                group.add(this.#createPowerLine(0.09, 0.36, -0.09,  0.5, 0.36, -0.09));
                break;
            case Side.Top:
                group.add(this.#createPowerLine( 0.09, 0.36, -0.09,  0.09, 0.36, -0.5));
                group.add(this.#createPowerLine(-0.09, 0.36, -0.09, -0.09, 0.36, -0.5));
                break;
            case Side.Bottom:
                group.add(this.#createPowerLine( 0.09, 0.36, 0.09,  0.09, 0.36, 0.5));
                group.add(this.#createPowerLine(-0.09, 0.36, 0.09, -0.09, 0.36, 0.5));
                break;
        }
    }

    #createPowerLine(x1, y1, z1, x2, y2, z2) {
        const points = [new THREE.Vector3(x1, y1, z1), new THREE.Vector3(x2, y2, z2)];
        const geometry = new THREE.BufferGeometry().setFromPoints(points);
        const line = new THREE.Line(geometry, powerLineMaterial);
        line.layers.set(1);
        return line;
    }
}
