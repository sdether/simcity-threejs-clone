export class SimObject {

  /**
   * @param {number} x The x-coordinate of the object 
   * @param {number} y The y-coordinate of the object
   */
  constructor(x = 0, y = 0) {
    this.name = 'SimObject';
    this.x = x;
    this.y = y;
  }

  /**
   * Updates the state of this object by one simulation step
   * @param {World} world
   */
  simulate(world) {
    // Override in subclass
  }

  dispose() {
  }

}