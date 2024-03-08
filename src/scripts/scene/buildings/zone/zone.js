import * as THREE from 'three';
import { DEG2RAD } from 'three/src/math/MathUtils.js';
import { Building } from '../building.js';
import {DevelopmentState} from "../../../model/buildings/zones/zone.js";
import {getTile} from "../../../sim/tileTools.js";

/**
 * Represents a zoned building such as residential, commercial or industrial
 */
export class Zone extends Building {
  /**
   * The mesh style to use when rendering
   */
  style = ['A', 'B', 'C'][Math.floor(3 * Math.random())];

  constructor(x = 0, y = 0) {
    super(x, y);

    // Randomize the building rotation
    this.rotation.y = 90 * Math.floor(4 * Math.random()) * DEG2RAD;
  }

  /**
   *
   * @param {World} world
   */
  refreshView(world) {
    let simBuilding = getTile(world, this.x,this.y).building
    let modelName;
    switch (simBuilding.development.state) {
      case DevelopmentState.underConstruction:
      case DevelopmentState.undeveloped:
        modelName = 'under-construction';
        break;
      default:
        modelName = `${simBuilding.type}-${this.style}${simBuilding.development.level}`;
        break;
    }

    let mesh = window.assetManager.getModel(modelName, this);

    // Tint building a dark color if it is abandoned
    if (simBuilding.development.state === DevelopmentState.abandoned) {
      mesh.traverse((obj) => {
        if (obj.material) {
          obj.material.color = new THREE.Color(0x707070);
        }
      });
    }
    
    this.setMesh(mesh);
  }

}