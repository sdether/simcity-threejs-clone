import {ZoneWithJobs} from "./zoneWithJobs.js";

export class CommerceState {
  capacity = 0;
}


export class CommercialZone extends ZoneWithJobs {

  commerce = new CommerceState();

  constructor(tile, type) {
    super(tile, type)
    this.name = generateBusinessName();
  }

  toHTML() {
    return super.toHTML() +  `
    <div class="info-heading">Details</div>
    <span class="info-label">Customer Capacity </span>
    <span class="info-value">${this.commerce.capacity}</span>
    <br>`;
  }

}

// Arrays of words for generating business names
const prefixes = ['Prime', 'Elite', 'Global', 'Exquisite', 'Vibrant', 'Luxury', 'Innovative', 'Sleek', 'Premium', 'Dynamic'];
const suffixes = ['Commerce', 'Trade', 'Marketplace', 'Ventures', 'Enterprises', 'Retail', 'Group', 'Emporium', 'Boutique', 'Mall'];
const businessSuffixes = ['LLC', 'Inc.', 'Co.', 'Corp.', 'Ltd.'];

// Function to generate a random commercial business name
function generateBusinessName() {
  const prefix = prefixes[Math.floor(Math.random() * prefixes.length)];
  const suffix = suffixes[Math.floor(Math.random() * suffixes.length)];
  const businessSuffix = businessSuffixes[Math.floor(Math.random() * businessSuffixes.length)];

  return prefix + ' ' + suffix + ' ' + businessSuffix;
}
