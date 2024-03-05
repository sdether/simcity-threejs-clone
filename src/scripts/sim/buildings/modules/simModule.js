import { World } from '../../world.js';

export class SimModule {
  /**
   * Simulates one day passing
   * @param {World} city
   */
  simulate(city) {
    // Implement in subclass
  }

  /**
   * Cleans up this module, disposing of any assets and unlinking any references
   */
  dispose() {
    // Implement in subclass
  }

  /**
   * Returns an HTML representation of this object
   * @returns {string}
   */
  toHTML() {
    // Implement in subclass
  }
}