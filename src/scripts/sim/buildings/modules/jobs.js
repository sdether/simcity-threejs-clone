import config from '../../../config.js';
import {SimModule} from './simModule.js';
import {ZoneWithJobs} from "../../../model/buildings/zones/zoneWithJobs.js";
import {DevelopmentState} from "../../../model/buildings/zones/zone.js";

export class JobsModule extends SimModule {

    /**
     * @param {World} world
     * @param {ZoneWithJobs} zone
     */
    simulate(world, zone) {
        // If building is abandoned, all workers are laid off and no
        // more workers are allowed to work here
        if (zone.development.state === DevelopmentState.abandoned) {
            this.#layOffWorkers(zone);
            return
        }
        let maxWorkers = this.#getMaxWorkers(zone);
        let availableJobs = maxWorkers - zone.workers.length;
        if (maxWorkers !== zone.maxWorkers || availableJobs !== zone.availableJobs) {
            zone.maxWorkers = maxWorkers;
            zone.availableJobs = availableJobs;
            zone.updated = zone.tile.updated = true;
        }
    }

    /**
     * Handles any clean up needed before a building is removed
     *
     * @param {World} world
     * @param {ZoneWithJobs} zone
     */
    dispose(world, zone) {
        this.#layOffWorkers(zone);
    }

    /**
     *
     * @param {ZoneWithJobs} zone
     * @returns {number}
     */
    #getMaxWorkers(zone) {
        // If building is not developed, there are no available jobs
        if (zone.development.state !== DevelopmentState.developed) {
            return 0;
        } else {
            return Math.pow(config.modules.jobs.maxWorkers, zone.development.level);
        }
    }

    /**
     * Lay off all existing workers
     *
     * @param {ZoneWithJobs} zone
     */
    #layOffWorkers(zone) {
        for (const worker of zone.workers) {
            worker.workplace = null;
        }
        zone.workers = [];
        zone.updated = zone.tile.updated = true;
    }
}