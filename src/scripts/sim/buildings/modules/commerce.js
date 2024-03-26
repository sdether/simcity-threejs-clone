import config from "../../../config.js";
import {CommercialZone} from "../../../model/buildings/zones/commercial.js";

export class CommerceModule {
    /**
     * @param {World} world
     * @param {CommercialZone} building
     */
    simulate(world, building) {
        if (building.development.level < 1) {
            return;
        }
        let capacity = config.modules.commerce.capacity * building.development.level;
        // owner counts as an extra worker
        let staffing_percentage = (building.workers.length + 1) / building.maxWorkers;
        building.commerce.capacity = Math.round(capacity * staffing_percentage);
    }
}