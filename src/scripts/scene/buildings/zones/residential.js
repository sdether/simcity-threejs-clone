import { Zone } from './zone.js';

export class ResidentialZone extends Zone {

  constructor(x, y) {
    super(x, y);
    this.name = generateBuildingName();
    this.type = BuildingType.residential;
  }

  /**
   * Handles any clean up needed before a building is removed
   */
  dispose() {
    this.residents.dispose();
    super.dispose();
  }

}
