import {Tile} from '../tile.js';
import {PowerConsumer} from "./power/powerConsumer.js";

let nextBuildingId = 0;

export class Building {

    id = nextBuildingId++;
    /**
     * @type {Tile}
     */
    tile;
    /**
     * The building type
     * @type {BuildingType}
     */
    type;
    /**
     * True if the terrain should not be rendered with this building type
     * @type {boolean}
     */
    hideTerrain = false;
    /**
     * The current status of the building
     * @type {string}
     */
    status = BuildingStatus.Ok;
    /**
     *
     * @type {boolean}
     */
    hasRoadAccess = false;
    /**
     *
     * @type {PowerConsumer}
     */
    power = null;
    /**
     * @type {boolean}
     */
    updated = true;

    /**
     *
     * @param {Tile} tile
     * @param {BuildingType} type
     * @param {PowerConsumer} power
     */
    constructor(tile, type, power = null) {
        this.tile = tile;
        this.power = power;
        this.tile.building = this;
        this.tile.updated = true;
        this.type = type;
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
      <span class="info-value">${this.hasRoadAccess}</span>
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
