import playIconUrl from '/icons/play-color.png';
import pauseIconUrl from '/icons/pause-color.png';

export class GameUI {
  /**
   * Currently selected tool
   * @type {string}
   */
  activeToolId = 'select';
  /**
   * @type {HTMLElement | null}
   */
  selectedControl = document.getElementById('button-select');
  /**
   * True if the game is currently paused
   * @type {boolean}
   */
  isPaused = false;

  get gameWindow() {
    return document.getElementById('render-target');
  }

  showLoadingText() {
    document.getElementById('loading').style.visibility = 'visible';
  }

  hideLoadingText() {
    document.getElementById('loading').style.visibility = 'hidden';
  }

  onToolSelected(event) {
    if (this.selectedControl) {
      this.selectedControl.classList.remove('selected');
    }
    this.selectedControl = event.target;
    this.selectedControl.classList.add('selected');
    this.activeToolId = this.selectedControl.getAttribute('data-type');
  }

  togglePause() {
    this.isPaused = !this.isPaused;
    if (this.isPaused) {
      document.getElementById('pause-button-icon').src = playIconUrl;
      document.getElementById('paused-text').style.visibility = 'visible';
    } else {
      document.getElementById('pause-button-icon').src = pauseIconUrl;
      document.getElementById('paused-text').style.visibility = 'hidden';
    }
  }

  /**
   * @param {string} cityName
   * @param {{ simTime: number, population: number }} stats
   */
  updateTitleBar(cityName, stats) {
    document.getElementById('city-name').innerHTML = cityName;
    document.getElementById('population-counter').innerHTML = stats.population.toString();
    const date = new Date('1/1/2023');
    date.setDate(date.getDate() + stats.simTime);
    document.getElementById('sim-time').innerHTML = date.toLocaleDateString();
  }

  /**
   * @param {{ terrain: string, building: object|null }|null} tileView
   */
  updateInfoPanel(tileView) {
    const infoElement = document.getElementById('info-panel');
    if (!tileView) {
      infoElement.style.visibility = 'hidden';
      infoElement.innerHTML = '';
      return;
    }
    infoElement.style.visibility = 'visible';
    infoElement.innerHTML = tileViewToHTML(tileView);
  }
}

/**
 * Generates an HTML string for a presentation-side tile view.
 * @param {{ x?: number, y?: number, terrain: string, building: object|null }} tileView
 * @returns {string}
 */
function tileViewToHTML(tileView) {
  let html = `
    <div class="info-heading">Tile</div>
    <span class="info-label">Terrain </span>
    <span class="info-value">${tileView.terrain}</span>
    <br>
  `;
  if (tileView.building) {
    html += buildingViewToHTML(tileView.building);
  }
  return html;
}

/**
 * @param {object} b  Building snapshot from WorldView
 * @returns {string}
 */
function buildingViewToHTML(b) {
  let html = `<div class="info-heading">Building</div>`;
  if (b.name) {
    html += `
      <span class="info-label">Name </span>
      <span class="info-value">${b.name}</span>
      <br>
    `;
  }
  html += `
    <span class="info-label">Type </span>
    <span class="info-value">${b.type}</span>
    <br>
    <span class="info-label">Status </span>
    <span class="info-value">${b.status}</span>
    <br>
  `;
  if (b.needsRoadAccess) {
    html += `
      <span class="info-label">Road Access </span>
      <span class="info-value">${b.hasRoadAccess}</span>
      <br>
    `;
  }
  if (b.power && b.power.required > 0) {
    html += `
      <span class="info-label">Power (kW)</span>
      <span class="info-value">${b.power.supplied}/${b.power.required}</span>
      <br>
    `;
  }
  if (b.development) {
    html += `
      <span class="info-label">State </span>
      <span class="info-value">${b.development.state}</span>
      <br>
      <span class="info-label">Level </span>
      <span class="info-value">${b.development.level}</span>
      <br>
    `;
  }
  if (b.residents !== undefined) {
    html += `<div class="info-heading">Residents (${b.residents.length}/${b.maxResidents})</div>`;
    html += '<ul class="info-citizen-list">';
    for (const r of b.residents) html += citizenToHTML(r);
    html += '</ul>';
  }
  if (b.workers !== undefined) {
    html += `<div class="info-heading">Workers (${b.workers.length}/${b.maxWorkers})</div>`;
    html += '<ul class="info-citizen-list">';
    for (const w of b.workers) html += citizenToHTML(w);
    html += '</ul>';
  }
  if (b.commerce !== undefined) {
    html += `
      <div class="info-heading">Details</div>
      <span class="info-label">Customer Capacity </span>
      <span class="info-value">${b.commerce.capacity}</span>
      <br>
    `;
  }
  return html;
}

/**
 * @param {{ name: string, age: number, state: string }} c
 * @returns {string}
 */
function citizenToHTML(c) {
  return `
    <li class="info-citizen">
      <span class="info-citizen-name">${c.name}</span>
      <br>
      <span class="info-citizen-details">
        <span>
          <img class="info-citizen-icon" src="/icons/calendar.png">
          ${c.age}
        </span>
        <span>
          <img class="info-citizen-icon" src="/icons/job.png">
          ${c.state}
        </span>
      </span>
    </li>
  `;
}

window.ui = new GameUI();
