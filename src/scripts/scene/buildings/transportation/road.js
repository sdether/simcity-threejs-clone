import {Building} from '../building.js';
import {DEG2RAD} from 'three/src/math/MathUtils.js';
import {World} from "../../../model/world.js";
import {getMatchingNeighbors, getTile} from "../../../sim/tileTools.js";

export class Road extends Building {

    constructor(x, y) {
        super(x, y);
        this.hideTerrain = true;
    }

    /**
     * Updates the road mesh based on which adjacent tiles are roads as well
     * @param {World} world
     */
    refreshView(world) {
        let simBuilding = getTile(world, this.x, this.y).building

        // Check which adjacent tiles are roads
        let {top, right, bottom, left} = getMatchingNeighbors(world, this.x, this.y, simBuilding.type);

        // Check all combinations
        // Four-way intersection
        if (top && bottom && left && right) {
            simBuilding.style = 'four-way';
            this.rotation.y = 0;
            // T intersection
        } else if (!top && bottom && left && right) { // bottom-left-right
            simBuilding.style = 'three-way';
            this.rotation.y = 0;
        } else if (top && !bottom && left && right) { // top-left-right
            simBuilding.style = 'three-way';
            this.rotation.y = 180 * DEG2RAD;
        } else if (top && bottom && !left && right) { // top-bottom-right
            simBuilding.style = 'three-way';
            this.rotation.y = 90 * DEG2RAD;
        } else if (top && bottom && left && !right) { // top-bottom-left
            simBuilding.style = 'three-way';
            this.rotation.y = 270 * DEG2RAD;
            // Corner
        } else if (top && !bottom && left && !right) { // top-left
            simBuilding.style = 'corner';
            this.rotation.y = 180 * DEG2RAD;
        } else if (top && !bottom && !left && right) { // top-right
            simBuilding.style = 'corner';
            this.rotation.y = 90 * DEG2RAD;
        } else if (!top && bottom && left && !right) { // bottom-left
            simBuilding.style = 'corner';
            this.rotation.y = 270 * DEG2RAD;
        } else if (!top && bottom && !left && right) { // bottom-right
            simBuilding.style = 'corner';
            this.rotation.y = 0;
            // Straight
        } else if (top && bottom && !left && !right) { // top-bottom
            simBuilding.style = 'straight';
            this.rotation.y = 0;
        } else if (!top && !bottom && left && right) { // left-right
            simBuilding.style = 'straight';
            this.rotation.y = 90 * DEG2RAD;
            // Dead end
        } else if (top && !bottom && !left && !right) { // top
            simBuilding.style = 'end';
            this.rotation.y = 180 * DEG2RAD;
        } else if (!top && bottom && !left && !right) { // bottom
            simBuilding.style = 'end';
            this.rotation.y = 0;
        } else if (!top && !bottom && left && !right) { // left
            simBuilding.style = 'end';
            this.rotation.y = 270 * DEG2RAD;
        } else if (!top && !bottom && !left && right) { // right
            simBuilding.style = 'end';
            this.rotation.y = 90 * DEG2RAD;
        }

        const mesh = window.assetManager.getModel(`road-${simBuilding.style}`, this);
        this.setMesh(mesh);
    }

}