import { Presentation } from '../../presentation.js';
import { Zone } from './zone.js';

export class IndustrialZone extends Zone {

  constructor(x, y) {
    super(x, y);
    this.name = generateBusinessName();
    this.type = BuildingType.industrial;
  }

  /**
   * Handles any clean up needed before a building is removed
   */
  dispose() {
    super.dispose();
  }

}

