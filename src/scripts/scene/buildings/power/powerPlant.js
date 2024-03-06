import { Building } from '../building.js';

export class PowerPlant extends Building {

  refreshView(simulation) {
    let simBuilding = simulation.getTile(this.x,this.y).building
    let mesh = window.assetManager.getModel(simBuilding.type, this);
    this.setMesh(mesh);
  }
}