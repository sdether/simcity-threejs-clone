import * as THREE from 'three';
import { AssetManager } from './assets/assetManager.js';
import { CameraManager } from './camera.js';
import { InputManager } from './input.js';
import { Presentation } from './scene/presentation.js';
import { DisplayObject } from './scene/displayObject.js';
import {Simulation} from "./sim/simulation.js";

/**
 * Orchestrates simulation, presentation, input and rendering.
 */
export class Game {
  /** @type {Presentation} */
  presentation;
  /** @type {Simulation} */
  simulation;
  /**
   * Object that currently has focus (hover).
   * @type {DisplayObject | null}
   */
  focusedObject = null;
  /** @type {InputManager} */
  inputManager;
  /**
   * Object that is currently selected.
   * @type {DisplayObject | null}
   */
  selectedObject = null;

  #prevLeftMouseDown = false;
  /** Maps "x,y" → toolId for tiles painted during the current mouse-down drag */
  #intentTiles = new Map();

  /**
   * Returns the presentation-side tile data for the selected tile,
   * without querying the simulation world.
   */
  get selectedTileView() {
    if (this.selectedObject == null) return null;
    return this.presentation.worldView.getTile(this.selectedObject.x, this.selectedObject.y);
  }

  constructor() {
    this.renderer = new THREE.WebGLRenderer({ antialias: true });
    this.scene = new THREE.Scene();

    this.inputManager = new InputManager(window.ui.gameWindow);
    this.cameraManager = new CameraManager(window.ui.gameWindow);

    this.renderer.setSize(window.ui.gameWindow.clientWidth, window.ui.gameWindow.clientHeight);
    this.renderer.setClearColor(0x000000, 0);
    this.renderer.shadowMap.enabled = true;
    this.renderer.shadowMap.type = THREE.PCFShadowMap;

    window.ui.gameWindow.appendChild(this.renderer.domElement);

    this.raycaster = new THREE.Raycaster();

    window.assetManager = new AssetManager(() => {
      window.ui.hideLoadingText();

      const size = 16;
      this.simulation = new Simulation(size);
      this.presentation = new Presentation();

      // subscribe fires a WorldSnapshot immediately, initialising the presentation
      this.simulation.subscribe(this.#simulationUpdated.bind(this));

      this.initialize(this.presentation);
      this.start();
    });

    window.addEventListener('resize', this.onResize.bind(this), false);
    window.addEventListener('keyup', this.#onKeyUp.bind(this), false);
  }

  initialize(presentation) {
    this.scene.clear();
    this.scene.add(presentation);
    this.#setupLights();
    this.#setupGrid(presentation);
  }

  #setupGrid(presentation) {
    const gridMaterial = new THREE.MeshBasicMaterial({
      color: 0x000000,
      map: window.assetManager.textures['grid'],
      transparent: true,
      opacity: 0.2
    });
    gridMaterial.map.repeat = new THREE.Vector2(presentation.size, presentation.size);
    gridMaterial.map.wrapS = presentation.size;
    gridMaterial.map.wrapT = presentation.size;

    const grid = new THREE.Mesh(
      new THREE.BoxGeometry(presentation.size, 0.1, presentation.size),
      gridMaterial
    );
    grid.position.set(presentation.size / 2 - 0.5, -0.04, presentation.size / 2 - 0.5);
    this.scene.add(grid);
  }

  #setupLights() {
    const sun = new THREE.DirectionalLight(0xffffff, 2);
    sun.position.set(-10, 20, 0);
    sun.castShadow = true;
    sun.shadow.camera.left = -20;
    sun.shadow.camera.right = 20;
    sun.shadow.camera.top = 20;
    sun.shadow.camera.bottom = -20;
    sun.shadow.mapSize.width = 2048;
    sun.shadow.mapSize.height = 2048;
    sun.shadow.camera.near = 10;
    sun.shadow.camera.far = 50;
    sun.shadow.normalBias = 0.01;
    this.scene.add(sun);
    this.scene.add(new THREE.AmbientLight(0xffffff, 0.5));
  }

  start() {
    this.renderer.setAnimationLoop(this.draw.bind(this));
    this.simulation.run();
  }

  stop() {
    this.simulation.halt();
    this.renderer.setAnimationLoop(null);
  }

  draw() {
    this.presentation.draw();
    this.updateFocusedObject();

    if (window.ui.isPaused) {
      this.simulation.halt();
    } else {
      this.simulation.run();
    }

    const isLeft = this.inputManager.isLeftMouseDown;

    if (isLeft) {
      this.#updateIntent();
    } else if (!isLeft && this.#prevLeftMouseDown) {
      this.#commitIntent();
    }
    this.#prevLeftMouseDown = isLeft;

    if (this.inputManager.isRightMouseDown) {
      this.maybeBulldoze();
    }
    this.renderer.render(this.scene, this.cameraManager.camera);
  }

  #simulationUpdated(events) {
    this.presentation.applyEvents(events);
    const { worldView } = this.presentation;
    window.ui.updateTitleBar(worldView.name, worldView.stats);
    window.ui.updateInfoPanel(this.selectedTileView);
  }

  /**
   * Intent phase: called every frame while left mouse is held.
   * Speculatively renders buildings without sending simulation commands.
   */
  #updateIntent() {
    if (!this.focusedObject) return;
    const { x, y } = this.focusedObject;
    const toolId = window.ui.activeToolId;

    if (toolId === 'select') {
      this.updateSelectedObject();
      window.ui.updateInfoPanel(this.selectedTileView);
      return;
    }

    if (toolId === 'bulldoze') {
      console.log('[CMD] bulldoze', { x, y });
      this.simulation.bulldoze(x, y);
      return;
    }

    const key = `${x},${y}`;
    if (this.#intentTiles.has(key)) return;

    this.#intentTiles.set(key, toolId);
    this.presentation.showIntent(x, y, toolId);
  }

  /**
   * Commit phase: called once on mouse-up.
   * Sends all accumulated intent tiles as simulation commands.
   */
  #commitIntent() {
    for (const [key, toolId] of this.#intentTiles) {
      const [x, y] = key.split(',').map(Number);
      console.log('[CMD] placeBuilding', { x, y, type: toolId });
      this.simulation.placeBuilding(x, y, toolId);
    }
    this.#intentTiles.clear();
  }

  maybeBulldoze() {
    if (this.focusedObject) {
      const { x, y } = this.focusedObject;
      console.log('[CMD] bulldoze', { x, y });
      this.simulation.bulldoze(x, y);
    }
  }

  updateSelectedObject() {
    if (this.selectedObject !== this.focusedObject) {
      this.selectedObject?.setSelected(false);
      this.selectedObject = this.focusedObject;
      this.selectedObject?.setSelected(true);
    }
  }

  updateFocusedObject() {
    const newObject = this.#raycast();
    if (newObject !== this.focusedObject) {
      this.focusedObject?.setFocused(false);
      this.focusedObject = newObject;
      this.focusedObject?.setFocused(true);
    }
  }

  #raycast() {
    const coords = {
      x: (this.inputManager.mouse.x / this.renderer.domElement.clientWidth) * 2 - 1,
      y: -(this.inputManager.mouse.y / this.renderer.domElement.clientHeight) * 2 + 1,
    };
    this.raycaster.setFromCamera(coords, this.cameraManager.camera);
    const intersections = this.raycaster.intersectObjects(this.presentation.root.children, true);
    return intersections.length > 0 ? intersections[0].object.userData : null;
  }

  #onKeyUp(event) {
    if (event.key === 'r') {
      this.simulation.requestFullRefresh();
    }
  }

  onResize() {
    this.cameraManager.resize(window.ui.gameWindow);
    this.renderer.setSize(window.ui.gameWindow.clientWidth, window.ui.gameWindow.clientHeight);
  }
}

window.onload = () => {
  window.game = new Game();
};
