import * as THREE from 'three';
import { DisplayObject } from '../displayObject.js';
import {BuildingStatus} from "../../sim/buildings/buildingStatus.js";

export class Building extends DisplayObject {
  /**
   * True if the terrain should not be rendered with this building type
   * @type {boolean}
   */
  hideTerrain = false;
  /**
   * Icon displayed when building status
   * @type {Sprite}
   */
  #statusIcon = new THREE.Sprite();

  constructor() {
    super();
    this.#statusIcon.visible = false;
    this.#statusIcon.material = new THREE.SpriteMaterial({ depthTest: false })
    this.#statusIcon.layers.set(1);
    this.#statusIcon.scale.set(0.5, 0.5, 0.5);
    this.add(this.#statusIcon);
  }
  
  refreshView(simulation) {
    let simBuilding = simulation.getTile(this.x,this.y).building
      switch(simBuilding.status) {
        case BuildingStatus.NoPower:
          this.#statusIcon.visible = true;
          this.#statusIcon.material.map = window.assetManager.statusIcons[BuildingStatus.NoPower];
          break;
        case BuildingStatus.NoRoadAccess:
          this.#statusIcon.visible = true;
          this.#statusIcon.material.map = window.assetManager.statusIcons[BuildingStatus.NoRoadAccess];
          break;
        default:
          this.#statusIcon.visible = false;
      }
    }


  dispose() {
    super.dispose();
  }
}