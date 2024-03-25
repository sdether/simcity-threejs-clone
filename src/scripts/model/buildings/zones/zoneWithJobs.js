import {DevelopmentState, Zone} from './zone.js';
import config from "../../../config.js";

export class ZoneWithJobs extends Zone {
  /**
   * @type {Citizen[]}
   */
  workers = [];
  maxWorkers = 0;

  get availableJobs() {
    return this.maxWorkers - this.workers.length
  }

  /**
   * Returns an HTML representation of this object
   * @returns {string}
   */
  toHTML() {
    let html = super.toHTML();
    html += `<div class="info-heading">Workers (${this.workers.length}/${this.maxWorkers})</div>`;

    html += '<ul class="info-citizen-list">';
    for (const worker of this.workers) {
      html += worker.toHTML();
    }
    html += '</ul>';

    return html;
  }
}