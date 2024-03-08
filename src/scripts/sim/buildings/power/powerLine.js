import { BuildingManager } from '../buildingManager.js';
import { BuildingType } from '../buildingType.js';

export class PowerLine extends BuildingManager {

  constructor(x, y) {
    super(x, y);
    this.type = BuildingType.powerLine;
    this.roadAccess.enabled = false;
  }

}