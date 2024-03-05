import * as THREE from 'three';
import { Building } from './buildings/building.js';
import { DisplayObject } from './displayObject.js';

export class Tile extends DisplayObject {
  /**
   * The type of terrain
   * @type {string}
   */
  terrain = 'grass';
  /**
   * The building on this tile
   * @type {Building?}
   */
  #building = null;

  constructor(x, y) {
    super(x, y);
   }

  /**
   * @type {Building}
   */
  get building() {
    return this.#building;
  }

  /**
   * @type {Building} value
   */
  setBuilding(value) {
    // Remove and dispose resources for existing building
    if (this.#building) {
      this.#building.dispose();
      this.remove(this.#building);
    }

    this.#building = value;

    // Add to scene graph
    if (value) {
      this.add(this.#building);
    }
  }

  refreshView(simulation) {
    this.building?.refreshView(simulation);
    if (this.building?.hideTerrain) {
      this.setMesh(null);
    } else {
      /**
       * @type {THREE.Mesh}
       */
      const mesh = window.assetManager.getModel(this.terrain, this);
      mesh.name = this.terrain;
      this.setMesh(mesh);
    }
  }
};