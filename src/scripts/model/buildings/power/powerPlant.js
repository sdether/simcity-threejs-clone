import { Building } from '../building.js';
import { BuildingType } from '../buildingType.js';
import {PowerConsumer} from "./powerConsumer.js";

export class PowerPlant extends Building {

  /**
   * Available units of power (kW)
   */
  powerCapacity = 100;

  /**
   * Consumed units of power
   */
  powerConsumed = 0;

  /**
   * Gets the amount of power available
   */
  get powerAvailable() {
    // Power plant must have road access in order to provide power
    if (this.hasRoadAccess) {
      return this.powerCapacity - this.powerConsumed;
    } else {
      return 0;
    }
  }

  constructor(tile, type) {
    super(tile, type, null, true)
  }

  /**
   * Returns an HTML representation of this object
   * @returns {string}
   */
  toHTML() {
    let html = super.toHTML();
    if (this.needsRoadAccess) {
      html += `
              <div class="info-heading">Building</div>
              <span class="info-label">Road Access </span>
              <span class="info-value">${this.hasRoadAccess}</span>
              <br>`;
    }
    html += `
      <div class="info-heading">Power</div>
      <span class="info-label">Power Capacity (kW)</span>
      <span class="info-value">${this.powerCapacity}</span>
      <br>
      <span class="info-label">Power Consumed (kW)</span>
      <span class="info-value">${this.powerConsumed}</span>
      <br>
      <span class="info-label">Power Available (kW)</span>
      <span class="info-value">${this.powerAvailable}</span>
      <br>
    `;
    return html;
  }
}