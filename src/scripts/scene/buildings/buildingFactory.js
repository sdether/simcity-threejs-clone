import {Road} from './transportation/road.js';
import {Building} from './building.js';
import {PowerPlant} from './power/powerPlant.js';
import {PowerLine} from './power/powerLine.js';
import {Zone} from "./zone/zone.js";
import {BuildingType} from "../../model/buildings/buildingType.js";

/**
 * Creates a new display building object for the given type.
 * Sets `buildingType` so the presentation layer can identify building types
 * without querying the sim world.
 * @param {number} x
 * @param {number} y
 * @param {BuildingType} type
 * @returns {Building}
 */
export function createBuilding(x, y, type) {
    let building;
    switch (type) {
        case BuildingType.residential:
        case BuildingType.commercial:
        case BuildingType.industrial:
            building = new Zone();
            break;
        case BuildingType.road:
            building = new Road();
            break;
        case BuildingType.powerPlant:
            building = new PowerPlant();
            break;
        case BuildingType.powerLine:
            building = new PowerLine();
            break;
        default:
            console.error(`${type} is not a recognized building type.`);
            return;
    }
    building.buildingType = type;
    return building;
}
