import {Building} from './buildings/building.js';
import {DisplayObject} from "../scene/displayObject.js";

let nextTileId = 0;

export class Tile {
    id = nextTileId++;
    x = 0;
    y = 0;

    /**
     * The type of terrain
     * @type {string}
     */
    terrain = 'grass';
    /**
     * The building on this tile
     * @type {Building?}
     */
    building = null;

    /**
     * @type {boolean}
     */
    updated = true;

    constructor(x, y, i = null) {
        this.x = x;
        this.y = y;
        this.name = `Tile-${this.x}-${this.y}`;
        if (i !== null) {
            let model_file = models[i];
            if(model_file) {
                this.building = new TileBuilding(this, model_file);
            }
        }
    }


    /**
     *
     * @returns {string} HTML representation of this object
     */
    toHTML() {
        let html = `
          <div class="info-heading">Tile</div>
          <span class="info-label">Coordinates </span>
          <span class="info-value">X: ${this.x}, Y: ${this.y}</span>
          <br>
          <span class="info-label">Terrain </span>
          <span class="info-value">${this.terrain}</span>
          <br>
        `;

        if (this.building) {
            html += this.building.toHTML();
        }

        return html;
    }
}

export class TileBuilding {
    constructor(tile, model_file) {
        this.tile = tile;
        this.model_file = model_file;
    }

    toHTML() {
        let html = `
          <div class="info-heading">Building</div>
          <span class="info-label">File </span>
          <span class="info-value">${this.model_file}</span>
          <br>`;
        return html;
    }


}

const models = [
    "building-bank.glb",
    "building-block-4floor-back.glb",
    "building-block-4floor-corner.glb",
    "building-block-4floor-front.glb",
    "building-block-4floor-short.glb",
    "building-block-5floor.glb",
    "building-block-5floor-corner.glb",
    "building-block-5floor-front.glb",
    "building-block-5floor-short.glb",
    "building-burger-joint.glb",
    "building-cabin-big.glb",
    "building-cafe.glb",
    "building-carwash.glb",
    "building-casino.glb",
    "building-cinema.glb",
    "building-firestation.glb",
    "building-hospital.glb",
    "building-hotel.glb",
    "building-house-block.glb",
    "building-house-block-big.glb",
    "building-house-block-old.glb",
    "building-house-family-large.glb",
    "building-house-family-small.glb",
    "building-house-modern.glb",
    "building-house-modern-big.glb",
    "building-mall.glb",
    "building-market-china.glb",
    "building-museum.glb",
    "building-office.glb",
    "building-office-balcony.glb",
    "building-office-big.glb",
    "building-office-pyramid.glb",
    "building-office-rounded.glb",
    "building-office-tall.glb",
    "building-policestation.glb",
    "building-port-sea.glb",
    "building-post.glb",
    "building-restaurant.glb",
    "building-school.glb",
    "building-shop-china.glb",
    "building-skyscraper.glb",
    "data-center.glb",
    "industry-building.glb",
    "industry-factory.glb",
    "industry-factory-hall.glb",
    "industry-factory-old.glb",
    "industry-refinery.glb",
    "industry-storage.glb",
    "industry-warehouse.glb",
    "nuclear-power-plant.glb",
    "skyscraper.glb",
    "skyscraper-huge.glb",
    "skyscraper-large.glb",
    "skyscraper-medium.glb",

]