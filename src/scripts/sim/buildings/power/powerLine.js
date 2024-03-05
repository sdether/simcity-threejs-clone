import { Building } from '../building.js';
import { BuildingType } from '../buildingType.js';

export class PowerLine extends Building {

  constructor(x, y) {
    super(x, y);
    this.type = BuildingType.powerLine;
    this.roadAccess.enabled = false;
  }

}