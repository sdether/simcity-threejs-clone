import { Building } from '../building.js';
import { BuildingType } from '../buildingType.js';

export class PowerPlant extends Building {

  constructor(x, y) {
    super(x, y);
    this.type = BuildingType.powerPlant;
  }

  refreshView() {
    let mesh = window.assetManager.getModel(this.type, this);
    this.setMesh(mesh);
  }
}