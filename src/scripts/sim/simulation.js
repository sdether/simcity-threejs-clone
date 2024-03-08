import {PowerService} from './services/power.js';
import {SimService} from './services/simService.js';
import {BuildingManager} from "./buildings/buildingManager.js";
import {CitizenManager} from "./citizenManager.js";
import {World} from "../model/world.js";
import {Tile} from "../model/tile.js";
import {getTile} from "./tileTools.js";

export const SimulationState = {
    Stopped: 'stopped',
    Running: 'running',
}

export class Simulation {

    state = SimulationState.Stopped;

    intervalId = 0;

    /**
     * List of services for the city
     * @type {SimService[]}
     */
    services = [];

    buildingManager = new BuildingManager()
    subscribers = [];

    constructor(size, name = 'My City') {
        this.world = new World(name, size)
        this.world.tiles = Array(size)
        for (let x = 0; x < size; x++) {
            this.world.tiles[x] = Array(size)
            for (let y = 0; y < size; y++) {
                this.world.tiles[x][y] = new Tile(x, y);
            }
        }

        this.services = [];
        this.services.push(new PowerService());

    }

    run() {
        if (this.state === SimulationState.Running) return;
        this.intervalId = setInterval(this.tick.bind(this), 1000);
        this.state = SimulationState.Running;
    }

    halt() {
        if (this.state === SimulationState.Stopped) return;
        clearInterval(this.intervalId);
        this.state = SimulationState.Stopped;
    }

    subscribe(subscriber) {
        this.subscribers.push(subscriber);
    }

    notifySubscribers(world) {
        for (const subscriber of this.subscribers) {
            subscriber(world);
        }
    }

    tick() {
        this.#cleanWorld();
        // Update services
        this.services.forEach((service) => service.simulate(this.world));
        for (let x = 0; x < this.world.size; x++) {
            for (let y = 0; y < this.world.size; y++) {
                let currentTile = this.world.tiles[x][y];
                this.buildingManager.simulate(this.world, currentTile);
            }
        }
        let citizens = this.world.citizens;
        this.world.citizens = [];
        for (const citizen of citizens) {
            if(CitizenManager.simulate(this.world, citizen)) {
                this.world.citizens.push(citizen)
            }
        }
        this.world.simTime++;
        this.notifySubscribers(this.world);
    }

    /**
     * Places a building at the specified coordinates if the
     * tile does not already have a building on it
     * @param {number} x
     * @param {number} y
     * @param {string} buildingType
     */
    placeBuilding(x, y, buildingType) {
        const buildingTile = getTile(this.world, x, y);
        this.buildingManager.create(buildingTile, buildingType);
        if(buildingTile && buildingTile.updated) {
            this.notifySubscribers(this.world);
        }
    }

    /**
     * Bulldozes the building at the specified coordinates
     * @param {number} x
     * @param {number} y
     */
    bulldoze(x, y) {
        const buildingTile = getTile(this.world, x, y);
        this.buildingManager.bulldoze(this.world, buildingTile);
        if(buildingTile && buildingTile.updated) {
            this.notifySubscribers(this.world);
        }
    }

    #cleanWorld() {
        for (const citizen of this.world.citizens) {
            citizen.updated = false;
        }
        for (let x = 0; x < this.world.size; x++) {
            for (let y = 0; y < this.world.size; y++) {
                let tile = this.world.tiles[x][y]
                tile.updated = false;
                if(tile.building) {
                    tile.building.updated = false;
                }
            }
        }
    }
}