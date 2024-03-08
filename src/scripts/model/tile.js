import { Building } from './buildings/building.js';

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

  constructor(x, y, building) {
    this.x = x;
    this.y = y;
    this.name = `Tile-${this.x}-${this.y}`;
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