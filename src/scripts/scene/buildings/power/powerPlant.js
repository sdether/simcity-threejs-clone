import { Building } from '../building.js';

export class PowerPlant extends Building {

  refreshView(simulation) {
    let mesh = window.assetManager.getModel(this.type, this);
    this.setMesh(mesh);
  }
}