import * as THREE from 'three';
import { DisplayObject } from '../displayObject.js';
import { BuildingStatus } from './buildingStatus';
import { PowerModule } from './modules/power';
import { RoadAccessModule } from './modules/roadAccess';

export class Building extends DisplayObject {
  /**
   * The building type
   * @type {string}
   */
  type = 'building';
  /**
   * True if the terrain should not be rendered with this building type
   * @type {boolean}
   */
  hideTerrain = false;
  /**
   * The current status of the building
   * @type {string}
   */
  status = BuildingStatus.Ok;
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
  
  /**
   * 
   * @param {*} status 
   */
  setStatus(status) {
    if (status !== this.status) {
      switch(status) {
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
  }

  dispose() {
    this.power.dispose();
    this.roadAccess.dispose();
    super.dispose();
  }
}