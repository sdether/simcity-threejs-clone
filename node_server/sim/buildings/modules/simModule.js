import { Simulation } from '../../simulation.js';

export class SimModule {

  // TODO: refactor that either modules attach their state lazily to buildings
  // or that buildings define state and we use that existence to run the module

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