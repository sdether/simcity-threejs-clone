import { Building } from '../building.js';
import { BuildingType } from '../buildingType.js';

const Side = {
  Left: 'left',
  Right: 'right',
  Top: 'top',
  Bottom: 'bottom'
}

export class PowerLine extends Building {

  constructor(x, y) {
    super(x, y);
    this.type = BuildingType.powerLine;
    this.roadAccess.enabled = false;
  }

}