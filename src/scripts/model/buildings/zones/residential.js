import { Zone } from './zone.js';
import { BuildingType } from '../buildingType.js';
import config from "../../../config.js";

export class ResidentialZone extends Zone {
  /**
   * @type {Citizen[]}
   */
  residents = [];
  maxResidents = 0;

  constructor(tile, type) {
    super(tile, type)
    this.name = generateBuildingName();
  }

  /**
   * Returns an HTML representation of this object
   * @returns {string}
   */
  toHTML() {
    let html = super.toHTML();
    html += `<div class="info-heading">Residents (${this.residents.length}/${this.maxResidents})</div>`;

    html += '<ul class="info-citizen-list">';
    for (const resident of this.residents) {
      html += resident.toHTML();
    }
    html += '</ul>';

    return html;
  }
}

// Arrays of different name components
const prefixes = ['Emerald', 'Ivory', 'Crimson', 'Opulent', 'Celestial', 'Enchanted', 'Serene', 'Whispering', 'Stellar', 'Tranquil'];
const suffixes = ['Tower', 'Residence', 'Manor', 'Court', 'Plaza', 'House', 'Mansion', 'Place', 'Villa', 'Gardens'];

// Function to generate a random building name
function generateBuildingName() {
  const prefix = prefixes[Math.floor(Math.random() * prefixes.length)];
  const suffix = suffixes[Math.floor(Math.random() * suffixes.length)];
  
  return prefix + ' ' + suffix;
}