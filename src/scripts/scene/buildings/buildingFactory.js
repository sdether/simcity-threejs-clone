import { Road } from './transportation/road.js';
import { Building } from './building.js';
import { PowerPlant } from './power/powerPlant.js';
import { PowerLine } from './power/powerLine.js';
import {BuildingType} from "../../sim/buildings/buildingType.js";
import {Zone} from "./zone/zone.js";

/**
 * Creates a new building object
 * @param {number} x The x-coordinate of the building
 * @param {number} y The y-coordinate of the building
 * @param {string} type The building type
 * @returns {Building} A new building object
 */
export function createBuilding(x, y, type) {
  switch (type) {
    case BuildingType.residential: 
    case BuildingType.commercial:
    case BuildingType.industrial:
      return new Zone();
    case BuildingType.road: 
      return new Road();
    case BuildingType.powerPlant:
      return new PowerPlant();
    case BuildingType.powerLine:
      return new PowerLine();
    default:
      console.error(`${type} is not a recognized building type.`);
  }
}