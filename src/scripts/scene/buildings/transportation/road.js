import {Building} from '../building.js';
import {DEG2RAD} from 'three/src/math/MathUtils.js';

export class Road extends Building {

    constructor(x, y) {
        super(x, y);
        this.hideTerrain = true;
    }

    /**
     * Updates the road mesh based on which adjacent tiles are also roads.
     * @param {{ terrain: string, building: object }} tileView
     * @param {import('../../worldView.js').WorldView} worldView
     */
    refreshView(tileView, worldView) {
        const {top, right, bottom, left} = worldView.getMatchingNeighbors(this.x, this.y, tileView.building.type);
        this.refreshFromNeighbors(top, right, bottom, left);
    }

    /**
     * Updates the road mesh given pre-computed neighbor connectivity.
     * Used by both the committed path (via refreshView) and the intent path
     * (where connectivity is derived from display state, not the sim world).
     * @param {boolean} top
     * @param {boolean} right
     * @param {boolean} bottom
     * @param {boolean} left
     */
    refreshFromNeighbors(top, right, bottom, left) {
        let style;
        let rotation = 0;

        if (top && bottom && left && right) {
            style = 'four-way';
        } else if (!top && bottom && left && right) {
            style = 'three-way'; rotation = 0;
        } else if (top && !bottom && left && right) {
            style = 'three-way'; rotation = 180 * DEG2RAD;
        } else if (top && bottom && !left && right) {
            style = 'three-way'; rotation = 90 * DEG2RAD;
        } else if (top && bottom && left && !right) {
            style = 'three-way'; rotation = 270 * DEG2RAD;
        } else if (top && !bottom && left && !right) {
            style = 'corner'; rotation = 180 * DEG2RAD;
        } else if (top && !bottom && !left && right) {
            style = 'corner'; rotation = 90 * DEG2RAD;
        } else if (!top && bottom && left && !right) {
            style = 'corner'; rotation = 270 * DEG2RAD;
        } else if (!top && bottom && !left && right) {
            style = 'corner'; rotation = 0;
        } else if (top && bottom && !left && !right) {
            style = 'straight'; rotation = 0;
        } else if (!top && !bottom && left && right) {
            style = 'straight'; rotation = 90 * DEG2RAD;
        } else if (top && !bottom && !left && !right) {
            style = 'end'; rotation = 180 * DEG2RAD;
        } else if (!top && bottom && !left && !right) {
            style = 'end'; rotation = 0;
        } else if (!top && !bottom && left && !right) {
            style = 'end'; rotation = 270 * DEG2RAD;
        } else {
            style = 'end'; rotation = 90 * DEG2RAD;
        }

        this.rotation.y = rotation;
        this.setMesh(window.assetManager.getModel(`road-${style}`, this));
    }
}
