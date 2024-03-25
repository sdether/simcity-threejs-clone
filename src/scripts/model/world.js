import {Tile} from "./tile.js";

export class World {
    /**
     * The current simulation time
     *   @type {number}
     */
    simTime = 0;
    /**
     * 2D array of tiles that make up the city
     * @type {Tile[][]}
     */
    tiles ;

    size = 0;

    name = null;

    stats = new Stats();

    /**
     * @type {Citizen[]}
     */
    citizens;

    constructor(name, size) {
        this.name = name;
        this.size = size;
        this.tiles = [];
        this.citizens = [];
        for (let x = 0; x < this.size; x++) {
            const column = [];
            for (let y = 0; y < this.size; y++) {
                column.push(null);
            }
            this.tiles.push(column);
        }
    }

}

export class Stats {
    demand =  new Demand();
}

export class Demand {
    residential = 0.0;
    commercial = 0.0;
    industrial = 0.0;
}