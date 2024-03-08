import config from '../../../config.js';
import {SimModule} from './simModule.js';
import {findTile} from "../../tileTools.js";

/**
 * Logic for determining whether or not a tile has road access
 */
export class RoadAccessModule extends SimModule {

    /**
     * @param {World} world
     * @param {Building} building
     */
    simulate(world, building) {
        const road = findTile(
            world,
            building.tile.x,
            building.tile.y,
            (tile) => {
                return tile.building?.type === 'road'
            },
            config.modules.roadAccess.searchDistance);

        let hasRoadAccess = (road !== null);
        if (building.hasRoadAccess !== hasRoadAccess) {
            building.hasRoadAccess = hasRoadAccess;
            building.updated = true;
            building.tile.updated = true;
        }
    }
}