import {BuildingType} from '../../model/buildings/buildingType.js';
import {SimService} from "./simService.js";
import {World} from "../../model/world.js";
import {getTile} from "../tileTools.js";

export class PowerService extends SimService {

    #at(tile) {
        return `p${tile.x}_${tile.y}`;
    }

    /**
     *
     * @param {World} world
     =   */
    simulate(world) {

        // Find all power plants in the world
        const powerPlantList = [];
        let powerConsumingBuildings = {};
        for (let x = 0; x < world.size; x++) {
            for (let y = 0; y < world.size; y++) {
                const tile = world.tiles[x][y];
                if (tile.building) {
                    let building = tile.building;
                    if (building.type === BuildingType.powerPlant) {
                        const powerPlant = building;
                        // Reset power consumption for each power plant
                        powerPlant.powerConsumed = 0;
                        // Create an object with the power plant, the search frontier, and a visited array
                        powerPlantList.push({
                            powerPlant,
                            frontier: [tile],
                            visited: []
                        });
                    } else if (building.power) {
                        powerConsumingBuildings[this.#at(tile)] = [building, building.power.copy()];
                    }
                }
            }
        }

        // If there are no power plants, return early
        if (powerPlantList.length === 0) {
            return;
        }

        // Power is allocated by performing a BFS starting at each power plants and
        // distributing power to buildings that require power. The search frontier
        // for each power plant is expanded simultaneously so the load is shared
        // as evenly as possible between power plants. Search is terminated when
        // all buildings have been visited or the power plant has no power remaining.

        let searching = true;
        while (searching) {
            searching = false;

            // Iterate over each power plant
            for (const item of powerPlantList) {
                const {powerPlant, frontier, visited} = item;

                // If power plant has no power left to give, ignore it
                if (powerPlant.powerAvailable === 0) continue;

                // If power plant has power available, find the next building on the search frontier
                if (frontier.length > 0) {
                    searching = true;

                    // Get the next tile
                    const tile = frontier.shift();
                    let at = this.#at(tile)
                    let data_at = powerConsumingBuildings[at];
                    if (data_at) {
                        let [_building, power] = data_at;

                        // Does this building need power?
                        if (power.supplied < power.required) {
                            const powerSupplied = Math.min(powerPlant.powerAvailable, power.required);
                            powerPlant.powerConsumed += powerSupplied;
                            power.supplied = powerSupplied;
                        }
                    }

                    visited.push(tile);

                    // Add adjacent buildings to the search frontier
                    const {x, y} = tile;

                    // Add neighboring tiles to search if
                    // 1) They haven't already been visited
                    // 2) The tile has a building (power can pass through non-powered buildings)
                    const shouldVisit = (tile) => tile && !visited.includes(tile) && tile.building;

                    let left = getTile(world, x - 1, y);
                    let right = getTile(world, x + 1, y);
                    let top = getTile(world, x, y - 1);
                    let bottom = getTile(world, x, y + 1);

                    if (shouldVisit(left)) {
                        frontier.push(left);
                    }
                    if (shouldVisit(right)) {
                        frontier.push(right);
                    }
                    if (shouldVisit(top)) {
                        frontier.push(top);
                    }
                    if (shouldVisit(bottom)) {
                        frontier.push(bottom);
                    }
                }
            }
        }

        for (let [building, power] of Object.values(powerConsumingBuildings)) {
            if (!building.power.isEqual(power)) {
                building.power = power;
                building.updated = building.tile.updated = true;
            }
        }
    }
}