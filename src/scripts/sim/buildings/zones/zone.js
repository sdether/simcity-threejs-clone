import { DEG2RAD } from 'three/src/math/MathUtils.js';
import { DevelopmentModule, DevelopmentState } from '../modules/development.js';
import { Building } from '../building.js';

/**
 * Represents a zoned building such as residential, commercial or industrial
 */
export class Zone extends Building {

  /**
   * @type {DevelopmentModule}
   */
  development = new DevelopmentModule(this);

  constructor(x = 0, y = 0) {
    super(x, y);
    
    this.name = 'Zone';
    this.power.required = 10;
    
  }

  simulate(city) {
    super.simulate(city);
    this.development.simulate(city);
  }

  /**
   * Returns an HTML representation of this object
   * @returns {string}
   */
  toHTML() {
    let html = super.toHTML();
    html += this.development.toHTML();
    return html;
  }
}