import {Building} from '../building.js';

export class PowerPlant extends Building {

    refreshForIntent() {
        super.refreshForIntent();
        this.setMesh(window.assetManager.getModel('power-plant', this));
    }

    /**
     * @param {{ terrain: string, building: object }} tileView
     * @param {import('../../worldView.js').WorldView} worldView
     */
    refreshView(tileView, worldView) {
        super.refreshView(tileView, worldView);
        const mesh = window.assetManager.getModel(tileView.building.type, this);
        this.setMesh(mesh);
    }
}
