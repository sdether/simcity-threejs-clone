import { Building } from './buildings/building.js';
import { SimObject } from './simObject.js';

export class Tile extends SimObject {
  /**
   * The type of terrain
   * @type {string}
   */
  terrain = 'grass';
  /**
   * The building on this tile
   * @type {Building?}
   */
  #building = null;

  constructor(x, y) {
    super(x, y);
    this.name = `Tile-${this.x}-${this.y}`;
  }

  /**
   * @type {Building}
   */
  get building() {
    return this.#building;
  }

  /**
   * @type {Building} value
   */
  setBuilding(value) {
    this.#building = value;
  }

  simulate(city) {
    this.building?.simulate(city);
  }

  /**
   * Gets the Manhattan distance between two tiles
   * @param {Tile} tile
   * @returns
   */
  distanceTo(tile) {
    return Math.abs(this.x - tile.x) + Math.abs(this.y - tile.y);
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
};