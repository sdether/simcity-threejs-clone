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
import {BuildingStatus} from "../../model/buildings/buildingStatus.js";

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
        if(!tile || !tile.building) {
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
                modules.push(this.roadAccessModule);
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
            module.simulate(world, building);
        }
        let newStatus = building.status;
        if(building.power && building.power.supplied < building.power.required) {
            newStatus = BuildingStatus.NoPower;
        } else if(!building.hasRoadAccess) {
            newStatus = BuildingStatus.NoRoadAccess;
        } else {
            newStatus = BuildingStatus.Ok;
        }
        if(newStatus !== building.status) {
            building.status = newStatus;
            building.updated = building.tile.updated = true;
        }
    }
}