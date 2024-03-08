import { Building } from '../building.js';

export class Road extends Building {
  constructor(tile, type) {
    super(tile, type)
    this.name = 'Road';
    this.style = 'straight';
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