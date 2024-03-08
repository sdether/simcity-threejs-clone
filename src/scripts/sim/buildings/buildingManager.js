import {RoadAccessModule} from './modules/roadAccess';
import {BuildingType} from "../../model/buildings/buildingType.js";
import {ResidentialZone} from "../../model/buildings/zones/residential.js";
import {CommercialZone} from "../../model/buildings/zones/commercial.js";
import {IndustrialZone} from "../../model/buildings/zones/industrial.js";
import {Road} from "../../model/buildings/transportation/road.js";
import {PowerPlant} from "../../model/buildings/power/powerPlant.js";
import {PowerLine} from "../../model/buildings/power/powerLine.js";
import {DevelopmentModule} from "./modules/development.js";
import {JobsModule} from "./modules/jobs.js";
import {ResidentsModule} from "./modules/residents.js";
import {World} from "../../model/world.js";
import {Tile} from "../../model/tile.js";

export class BuildingManager {

    developmentModule = new DevelopmentModule()
    jobsModule = new JobsModule()
    residentsModule = new ResidentsModule()
    roadAccessModule = new RoadAccessModule()

    create(tile, type) {
        if(!tile || tile.building) {
            return;
        }
        switch (type) {
            case BuildingType.residential:
                return new ResidentialZone(tile, type);
            case BuildingType.commercial:
                return new CommercialZone(tile, type);
            case BuildingType.industrial:
                return new IndustrialZone(tile, type);
            case BuildingType.road:
                return new Road(tile, type);
            case BuildingType.powerPlant:
                return new PowerPlant(tile, type);
            case BuildingType.powerLine:
                return new PowerLine(tile, type);
            default:
                console.error(`${type} is not a recognized building type.`);
        }
    }

    bulldoze(world, tile) {
        if(!tile || tile.building) {
            return;
        }
        let building = tile.building;
        let modules = this.#getModules(building);
        for (const module of modules) {
            module.dispose(world, building)
        }
        tile.building = null;
        tile.updated = true;
    }

    #getModules(building) {
        let modules = [];
        switch (building.type) {
            case BuildingType.residential:
                modules.push(this.roadAccessModule);
                modules.push(this.developmentModule);
                modules.push(this.residentsModule);
                break;
            case BuildingType.commercial:
                modules.push(this.roadAccessModule);
                modules.push(this.developmentModule);
                modules.push(this.jobsModule);
                break;
            case BuildingType.industrial:
                modules.push(this.roadAccessModule);
                modules.push(this.developmentModule);
                modules.push(this.jobsModule);
                break;
            case BuildingType.road:
                break;
            case BuildingType.powerPlant:
                break;
            case BuildingType.powerLine:
                break;
            default:
                console.error(`${building.type} is not a recognized building type.`);
        }
        return modules;
    }

    /**
     *
     * @param {World} world
     * @param {Tile} tile
     */
    simulate(world, tile) {
        if(!tile || !tile.building) {
            return;
        }
        let building = tile.building;
        let modules = this.#getModules(building);
        for (const module of modules) {
            module.simulate(building);
        }
        // if (!this.power.isFullyPowered) {
        //     this.setStatus(BuildingStatus.NoPower);
        // } else if (!this.roadAccess.value) {
        //     this.setStatus(BuildingStatus.NoRoadAccess);
        // } else {
        //     this.setStatus(null);
        // }
    }

    /**
     * Returns an HTML representation of this object
     * @returns {string}
     */
    toHTML() {
        let html = `
      <div class="info-heading">Building</div>
      <span class="info-label">Name </span>
      <span class="info-value">${this.name}</span>
      <br>
      <span class="info-label">Type </span>
      <span class="info-value">${this.type}</span>
      <br>
      <span class="info-label">Road Access </span>
      <span class="info-value">${this.roadAccess.value}</span>
      <br>`;

        if (this.power.required > 0) {
            html += `
        <span class="info-label">Power (kW)</span>
        <span class="info-value">${this.power.supplied}/${this.power.required}</span>
        <br>`;
        }
        return html;
    }
}