import {Building} from '../building.js';
import {PowerConsumer} from "../power/powerConsumer.js";

export const DevelopmentState = {
  abandoned: 'abandoned',
  developed: 'developed',
  underConstruction: 'under-construction',
  undeveloped: 'undeveloped',
};

export class Development {
  state = DevelopmentState.undeveloped;
  level = 0;
  abandonmentCounter = 0;
  constructionCounter = 0;
  maxLevel = 3;
}


/**
 * Represents a zoned building such as residential, commercial or industrial
 */
export class Zone extends Building {

  development = new Development();

  constructor(tile, type) {
    super(tile, type, new PowerConsumer(10), true)
  }

  /**
   * Returns an HTML representation of this object
   * @returns {string}
   */
  toHTML() {
    let html = super.toHTML() +  `
        <span class="info-label">State </span>
        <span class="info-value">${this.development.state}</span>
        <br>
        <span class="info-label">Level </span>
        <span class="info-value">${this.development.level}</span>
        <br>`;
    return html;
  }
}