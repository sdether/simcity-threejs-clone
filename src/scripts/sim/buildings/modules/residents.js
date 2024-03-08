import config from '../../../config.js';
import { SimModule } from './simModule.js';
import {ResidentialZone} from "../../../model/buildings/zones/residential.js";
import {DevelopmentState} from "../../../model/buildings/zones/zone.js";
import {Citizen} from "../../../model/citizen.js";

/**
 * Logic for residents moving into and out of a building
 */
export class ResidentsModule extends SimModule {

  /**
   * @param {World} world
   * @param {ResidentialZone} zone
   */
  simulate(world, zone) {
    // If building is abandoned, all residents are evicted and no more residents are allowed to move in.
    if (zone.development.state === DevelopmentState.abandoned && zone.residents.length > 0) {
      this.#evictAll(world, zone);
      return;
    }
    let updated = false;
    if (zone.development.state === DevelopmentState.developed) {
      let maximum =  Math.pow(config.modules.residents.maxResidents, zone.development.level);
      if( maximum !== zone.maxResidents) {
        zone.maxResidents = maximum;
        updated = true;
      }
      // Move in new residents if there is room
      if (zone.residents.length < maximum && Math.random() < config.modules.residents.residentMoveInChance) {
        let resident = new Citizen();
        resident.residence = zone;
        zone.residents.push(resident);
        world.citizens.push(resident);
        updated = true;
      }
    }
    if(updated) {
      zone.updated = zone.tile.updated = true;
    }
  }

  /**
   * Evicts all residents from the building
   * @param {World} world
   * @param {ResidentialZone} zone
   */
  #evictAll(world, zone) {
    if(zone.residents.length === 0) {
      return;
    }
    for (const resident of zone.residents) {
      resident.workplace = null;
    }
    zone.residents = [];
    zone.updated = zone.tile.updated = true;
  }

  /**
   * Handles any clean up needed before a building is removed
   */
  /**
   * @param {World} world
   * @param {ResidentialZone} zone
   */
  dispose(world, zone) {
    this.#evictAll(world, zone);
  }
}