import {CommercialZone} from './buildings/zones/commercial.js';
import {IndustrialZone} from './buildings/zones/industrial.js';
import {ResidentialZone} from './buildings/zones/residential.js';
import config from '../config.js';

let nextCitizenId = 0;

export const CitizenState = {
    idle: 'idle',
    school: 'school',
    employed: 'employed',
    unemployed: 'unemployed',
    retired: 'retired',
}

export class Citizen {
    id = nextCitizenId++;

    state = CitizenState.idle;
    stateCounter = 0;
    age = 0;
    /**
     * Reference to the building the citizen lives at
     * @type {ResidentialZone}
     */
    residence = null;

    /**
     * Reference to the building the citizen works at
     * @type {CommercialZone | IndustrialZone}
     */
    workplace = null;

    /**
     * @type {boolean}
     */
    updated = true;

    constructor() {

        this.name = generateRandomName();
        /**
         * Age of the citizen in years
         * @type {number}
         */
        this.age = 1 + Math.floor(100 * Math.random());

        if (this.age < config.citizen.minWorkingAge) {
            this.state = CitizenState.school;
        } else if (this.age >= config.citizen.retirementAge) {
            this.state = CitizenState.retired;
        } else {
            this.state = CitizenState.unemployed;
        }
    }

    /**
     * Returns an HTML representation of this object
     * @returns {string}
     */
    toHTML() {
        return `
      <li class="info-citizen">
        <span class="info-citizen-name">${this.name}</span>
        <br>
        <span class="info-citizen-details">
          <span>
            <img class="info-citizen-icon" src="/icons/calendar.png">
            ${this.age} 
          </span>
          <span>
            <img class="info-citizen-icon" src="/icons/job.png">
            ${this.state}
          </span>
        </span>
      </li>
    `;
    }
}

function generateRandomName() {
    const firstNames = [
        'Emma', 'Olivia', 'Ava', 'Sophia', 'Isabella',
        'Liam', 'Noah', 'William', 'James', 'Benjamin',
        'Elizabeth', 'Margaret', 'Alice', 'Dorothy', 'Eleanor',
        'John', 'Robert', 'William', 'Charles', 'Henry',
        'Alex', 'Taylor', 'Jordan', 'Casey', 'Robin'
    ];

    const lastNames = [
        'Smith', 'Johnson', 'Williams', 'Jones', 'Brown',
        'Davis', 'Miller', 'Wilson', 'Moore', 'Taylor',
        'Anderson', 'Thomas', 'Jackson', 'White', 'Harris',
        'Clark', 'Lewis', 'Walker', 'Hall', 'Young',
        'Lee', 'King', 'Wright', 'Adams', 'Green'
    ];

    const randomFirstName = firstNames[Math.floor(Math.random() * firstNames.length)];
    const randomLastName = lastNames[Math.floor(Math.random() * lastNames.length)];

    return randomFirstName + ' ' + randomLastName;
}