import { SimObject } from '../simObject';
import { BuildingStatus } from './buildingStatus';
import { PowerModule } from './modules/power';
import { RoadAccessModule } from './modules/roadAccess';

export class Building extends SimObject {
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
   * @type {PowerModule}
   */
  power = new PowerModule(this);
  /**
   * @type {RoadAccessModule}
   */
  roadAccess = new RoadAccessModule(this);
  /**
   * The current status of the building
   * @type {string}
   */
  status = BuildingStatus.Ok;

  constructor(x, y) {
    super(x, y);
  }

  /**
   * 
   * @param {*} status 
   */
  setStatus(status) {
    this.status = status
  }

  simulate(city) {
    super.simulate(city);
    
    this.power.simulate(city);
    this.roadAccess.simulate(city);

    if (!this.power.isFullyPowered) {
      this.setStatus(BuildingStatus.NoPower);
    } else if (!this.roadAccess.value) {
      this.setStatus(BuildingStatus.NoRoadAccess);
    } else {
      this.setStatus(null);
    }
  }

  dispose() {
    this.power.dispose();
    this.roadAccess.dispose();
    super.dispose();
  }

  /**
   * Returns an HTML representation of this object
   * @returns {string}
   */
  toHTML() {
    let html = `
      <div class="info-heading">Building</div>
      <span class="info-label">Name </span>
      <span class="info-value">${this.name}</span>
      <br>
      <span class="info-label">Type </span>
      <span class="info-value">${this.type}</span>
      <br>
      <span class="info-label">Road Access </span>
      <span class="info-value">${this.roadAccess.value}</span>
      <br>`;

    if (this.power.required > 0) {
      html += `
        <span class="info-label">Power (kW)</span>
        <span class="info-value">${this.power.supplied}/${this.power.required}</span>
        <br>`;
    } 
    return html;
  }
}