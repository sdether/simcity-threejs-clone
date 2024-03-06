let nextObjectId = 0;
export class SimObject {
  id = nextObjectId++;
  x = 0;
  y = 0;
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
   * @param {Simulation} simulation
   */
  simulate(simulation) {
    // Override in subclass
  }

  dispose() {
  }

  toHTML() {

  }
}