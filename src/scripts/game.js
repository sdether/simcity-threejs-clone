import * as THREE from 'three';
import { AssetManager } from './assets/assetManager.js';
import { CameraManager } from './camera.js';
import { InputManager } from './input.js';
import { Presentation } from './scene/presentation.js';
import { DisplayObject } from './scene/displayObject.js';
import {Simulation} from "./sim/simulation.js";
import {getTile} from "./sim/tileTools.js";

/** 
 * Manager for the Three.js scene. Handles rendering of a `City` object
 */
export class Game {
  /**
   * @type {Presentation}
   */
  presentation;
  /**
   * @type {Simulation}
   */
  simulation;
  /**
   * Object that currently hs focus
   * @type {DisplayObject | null}
   */
  focusedObject = null;
  /**
   * Class for managing user input
   * @type {InputManager}
   */
  inputManager;
  /**
   * Object that is currently selected
   * @type {DisplayObject | null}
   */
  selectedObject = null;

  get selectedTile() {
    if( this.selectedObject == null) {
      return null
    }
    return getTile(this.simulation.world, this.selectedObject.x, this.selectedObject.y)
  }
  constructor() {

    this.renderer = new THREE.WebGLRenderer({ 
      antialias: true
    });
    this.scene = new THREE.Scene();

    this.inputManager = new InputManager(window.ui.gameWindow);
    this.cameraManager = new CameraManager(window.ui.gameWindow);

    // Configure the renderer
    this.renderer.setSize(window.ui.gameWindow.clientWidth, window.ui.gameWindow.clientHeight);
    this.renderer.setClearColor(0x000000, 0);
    this.renderer.shadowMap.enabled = true;
    this.renderer.shadowMap.type = THREE.PCFShadowMap;

    // Add the renderer to the DOM
    window.ui.gameWindow.appendChild(this.renderer.domElement);

    // Variables for object selection
    this.raycaster = new THREE.Raycaster();

    /**
     * Global instance of the asset manager
     */
    window.assetManager = new AssetManager(() => {
      window.ui.hideLoadingText();
      let size = 11;
      this.simulation = new Simulation(size);
      this.simulation.subscribe(this.simulationUpdated.bind(this))
      this.presentation = new Presentation(this.simulation.world);

      this.initialize(this.presentation);
      this.start();

    });

    window.addEventListener('resize', this.onResize.bind(this), false);
  }

  /**
   * Initalizes the scene, clearing all existing assets
   */
  initialize(presentation) {
    this.scene.clear();
    this.scene.add(presentation);
    this.#setupLights();
    this.#setupGrid(presentation);
  }

  #setupGrid(city) {
    // Add the grid
    const gridMaterial = new THREE.MeshBasicMaterial({ 
      color: 0x000000,
      map: window.assetManager.textures['grid'],
      transparent: true,
      opacity: 0.2
    });
    gridMaterial.map.repeat = new THREE.Vector2(city.size, city.size);
    gridMaterial.map.wrapS = city.size;
    gridMaterial.map.wrapT = city.size;

    const grid = new THREE.Mesh(
      new THREE.BoxGeometry(city.size, 0.1, city.size),
      gridMaterial
    );
    grid.position.set(city.size / 2 - 0.5, -0.04, city.size / 2 - 0.5);
    this.scene.add(grid);
  }

  /**
   * Setup the lights for the scene
   */
  #setupLights() {
    const sun = new THREE.DirectionalLight(0xffffff, 2)
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
  
  /**
   * Starts the renderer
   */
  start() {
    this.renderer.setAnimationLoop(this.draw.bind(this));
    this.simulation.run()
  }

  /**
   * Stops the renderer
   */
  stop() {
    this.simulation.halt()
    this.renderer.setAnimationLoop(null);
  }

  /**
   * Render the contents of the scene
   */
  draw() {
    this.presentation.draw();
    this.updateFocusedObject();
    if(window.ui.isPaused) {
      this.simulation.halt()
    } else {
      this.simulation.run()
    }
    if (this.inputManager.isLeftMouseDown) {
      this.useTool();
    } else if(this.inputManager.isRightMouseDown) {
      this.maybeBulldoze();
    }

    this.renderer.render(this.scene, this.cameraManager.camera);
  }

  simulationUpdated(world) {
    this.presentation.update(world)
    window.ui.updateTitleBar(this);
    window.ui.updateInfoPanel(this.selectedTile);
  }


  /**
   * Bulldoze tile under cursor
   */
  maybeBulldoze() {
    if (this.focusedObject) {
      const { x, y } = this.focusedObject;
      this.simulation.bulldoze(x, y);
    }
  }

  /**
   * Uses the currently active tool
   */
  useTool() {
    switch (window.ui.activeToolId) {
      case 'select':
        this.updateSelectedObject();
        window.ui.updateInfoPanel(this.selectedTile);
        break;
      case 'bulldoze':
        if (this.focusedObject) {
          const { x, y } = this.focusedObject;
          this.simulation.bulldoze(x, y);
        }
        break;
      default:
        if (this.focusedObject) {
          const { x, y } = this.focusedObject;
          this.simulation.placeBuilding(x, y, window.ui.activeToolId);
        }
        break;
    }
  }
  
  /**
   * Sets the currently selected object and highlights it
   */
  updateSelectedObject() {
    if(this.selectedObject !== this.focusedObject) {
      this.selectedObject?.setSelected(false);
      this.selectedObject = this.focusedObject;
      this.selectedObject?.setSelected(true);
    }
  }

  /**
   * Sets the object that is currently highlighted
   */
  updateFocusedObject() {  
    const newObject = this.#raycast();
    if (newObject !== this.focusedObject) {
      this.focusedObject?.setFocused(false);
      this.focusedObject = newObject;
      this.focusedObject?.setFocused(true);
    }
  }

  /**
   * Gets the mesh currently under the mouse cursor. If there is nothing under
   * the mouse cursor, returns null
   * @param {MouseEvent} event Mouse event
   * @returns {THREE.Mesh | null}
   */
  #raycast() {
    let coords = {
      x: (this.inputManager.mouse.x / this.renderer.domElement.clientWidth) * 2 - 1,
      y: -(this.inputManager.mouse.y / this.renderer.domElement.clientHeight) * 2 + 1
    };

    this.raycaster.setFromCamera(coords, this.cameraManager.camera);

    let intersections = this.raycaster.intersectObjects(this.presentation.root.children, true);
    if (intersections.length > 0) {
      // The SimObject attached to the mesh is stored in the user data
      const selectedObject = intersections[0].object.userData;
      return selectedObject;
    } else {
      return null;
    }
  }

  /**
   * Resizes the renderer to fit the current game window
   */
  onResize() {
    this.cameraManager.resize(window.ui.gameWindow);
    this.renderer.setSize(window.ui.gameWindow.clientWidth, window.ui.gameWindow.clientHeight);
  }
}

// Create a new game when the window is loaded
window.onload = () => {
  window.game = new Game();
}