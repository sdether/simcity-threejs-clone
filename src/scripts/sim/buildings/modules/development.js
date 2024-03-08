import config from '../../../config.js';
import {DevelopmentState, Zone} from '../../../model/buildings/zones/zone.js';
import { SimModule } from './simModule.js';

export class DevelopmentModule extends SimModule {

  /**
   * @param {World} world
   * @param {Zone} zone
   */
  simulate(world, zone) {
    this.#checkAbandonmentCriteria(zone);

    switch (zone.development.state) {
      case DevelopmentState.undeveloped:
        if (this.#checkDevelopmentCriteria(zone) &&
          Math.random() < config.modules.development.redevelopChance) {
          zone.development.state = DevelopmentState.underConstruction;
          zone.development.constructionCounter = 0;
        }
        break;
      case DevelopmentState.underConstruction:
        if (++zone.development.constructionCounter === config.modules.development.constructionTime) {
          zone.development.state = DevelopmentState.developed;
          zone.development.level = 1;
          zone.development.constructionCounter = 0;
        }
        break;
      case DevelopmentState.developed:
        if (zone.development.abandonmentCounter > config.modules.development.abandonThreshold) {
          if (Math.random() < config.modules.development.abandonChance) {
            zone.development.state = DevelopmentState.abandoned;
          }
        } else {
          if (this.level < this.maxLevel && Math.random() < config.modules.development.levelUpChance) {
            this.level++;
          }
        }
        break;
      case DevelopmentState.abandoned:
        if (zone.development.abandonmentCounter === 0) {
          if (Math.random() < config.modules.development.redevelopChance) {
            zone.development.state = DevelopmentState.developed;
          }
        }
        break;
    }
    // TODO: handle updated state
  }

  #checkDevelopmentCriteria(zone) {
    return (
        zone.hasRoadAccess &&
        zone.power.supplied >= zone.power.required
    );
  }

  #checkAbandonmentCriteria(zone) {
    if (!this.#checkDevelopmentCriteria(zone)) {
      zone.development.abandonmentCounter++;
    } else {
      zone.development.abandonmentCounter = 0;
    }
  }
}