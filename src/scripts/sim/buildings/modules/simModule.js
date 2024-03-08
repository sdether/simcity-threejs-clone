import { Simulation } from '../../simulation.js';

export class SimModule {
  /**
   * @param {World} world
   * @param {Building} building
   */
  simulate(world, building) {
    // Implement in subclass
  }

  /**
   * @param {World} world
   * @param {Building} building
   */
  dispose(world, building) {
    // default does nothing
  }
}