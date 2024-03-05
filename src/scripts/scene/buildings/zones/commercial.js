import { Presentation } from '../../presentation.js';
import { Zone } from './zone.js';
import { JobsModule } from '../modules/jobs.js';
import { BuildingType } from '../buildingType.js';

export class CommercialZone extends Zone {

  constructor(x, y) {
    super(x, y);
    this.name = generateBusinessName();
    this.type = BuildingType.commercial;
  }


  /**
   * Handles any clean up needed before a building is removed
   */
  dispose() {
    super.dispose();
  }

}
