import { Building } from '../building.js';
import { BuildingType } from '../buildingType.js';

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

  /**
   * Returns an HTML representation of this object
   * @returns {string}
   */
  toHTML() {
    let html = super.toHTML();
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