import {Building} from '../building.js';
import {getTile} from "../../../sim/tileTools.js";
import {World} from "../../../model/world.js";

export class PowerPlant extends Building {

    /**
     *
     * @param {World} world
     */
    refreshView(world) {
        let simBuilding = getTile(world, this.x, this.y).building
        let mesh = window.assetManager.getModel(simBuilding.type, this);
        this.setMesh(mesh);
    }
}