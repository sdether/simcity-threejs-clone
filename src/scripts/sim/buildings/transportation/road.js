import { Building } from '../building.js';

export class Road extends Building {
  constructor(x, y) {
    super(x, y);
    this.type = 'road';
    this.name = 'Road';
    this.style = 'straight';
    this.roadAccess.enabled = false;
  }

  /**
   * Returns an HTML representation of this object
   * @returns {string}
   */
  toHTML() {
    let html = super.toHTML();
    html += `
    <span class="info-label">Style </span>
    <span class="info-value">${this.style}</span>
    <br>
    `;
    return html;
  }
}