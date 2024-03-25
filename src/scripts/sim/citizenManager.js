import config from '../config.js';
import {CitizenState} from "../model/citizen.js";
import {findTile} from "./tileTools.js";
import {BuildingType} from "../model/buildings/buildingType.js";

export class CitizenManager {

    /**
     *
     * @param {World} world
     * @param {Citizen} citizen
     */
    static simulate(world, citizen) {
        if (citizen.residence === null) {
            if (citizen.workplace) {
                citizen.workplace.workers = citizen.workplace.workers.filter(
                    c => c.id !== citizen.id
                );
                citizen.workplace = null;

            }
            return false;
        }
        switch (citizen.state) {
            case CitizenState.idle:
            case CitizenState.school:
            case CitizenState.retired:
                // Action - None

                // Transitions - None

                break;
            case CitizenState.unemployed:
                // Action - Look for a job
                this.#findJob(world, citizen);

                // Transitions
                if (citizen.workplace) {
                    citizen.state = CitizenState.employed;
                    citizen.updated = true;
                }

                break;
            case CitizenState.employed:
                // Actions - None

                // Transitions
                if (!citizen.workplace) {
                    citizen.state = CitizenState.unemployed;
                    citizen.updated = true;
                }

                break;
            default:
                console.error(`Citizen ${citizen.id} is in an unknown state (${citizen.state})`);
        }
        return true;
    }

    /**
     *
     * @param {World} world
     * @param {Citizen} citizen
     */
    static #findJob(world, citizen) {
        const tile = findTile(world, citizen.residence.tile.x, citizen.residence.tile.y,
            (tile) => {
                // Search for an industrial or commercial building with at least one available job
                if (tile.building?.type === BuildingType.industrial ||
                    tile.building?.type === BuildingType.commercial) {
                    if (tile.building.availableJobs > 0) {
                        return true;
                    }
                }

                return false;
            }, config.citizen.maxJobSearchDistance);
        // TODO: feels like this should happen in `simulate()`
        if (tile) {
            // Employ the citizen at the building
            tile.building.workers.push(citizen);
            citizen.workplace = tile.building;
        }
    }
}