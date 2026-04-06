# CitySim.js — Claude Code Guide

## Project Goal

This is a fork of Daniel Greenheck's CitySim.js, refactored toward a thin presentation layer for an **out-of-process city simulator**. The end goal is clean separation between presentation/interaction and simulation so that state changes can flow as events across a process boundary.

## Commands

```bash
npm run dev      # Dev server at http://127.0.0.1:3000
npm run build    # Production build → ./dist
npm run preview  # Preview production build
```

No test runner, no linter configured.

## Architecture

The codebase follows a **CQRS** pattern. Simulation emits events; presentation maintains its own read model and sends commands. The two layers never share object references.

### Model (`src/scripts/model/`)
Pure data objects — no logic, no Three.js. `World` holds a 2D tile grid and citizen list. `Tile` holds a building reference. Buildings use an inheritance hierarchy: `Building` → `Zone` / `Road` / `PowerPlant` / etc. Only the simulation layer reads/writes these.

### Simulation (`src/scripts/sim/`)
Business logic with no Three.js dependency. Runs on a 1-second `setInterval` tick. Key components:
- `simulation.js` — tick engine; emits event arrays to subscribers; fires a `WorldSnapshot` immediately on `subscribe()`
- `worldEvents.js` — event factories: `worldSnapshotEvent`, `tileChangedEvent`, `statsChangedEvent`. Each produces plain data — no object references
- `buildingManager.js` — factory that composes buildings from pluggable `SimModule` instances
- `modules/` — pluggable per-building behaviors: `development`, `residents`, `jobs`, `commerce`, `roadAccess`
- `services/power.js` — BFS power grid distribution
- `citizenManager.js` — citizen state machine (idle → school → unemployed → employed → retired)

### Presentation (`src/scripts/scene/`)
Three.js rendering only. Never imports from `sim/` (except the event-type constants in `worldEvents.js`) and never reads the sim `World` object.
- `worldView.js` — presentation-side read model (`WorldView`). Applies incoming events. Provides `getTile()` and `getMatchingNeighbors()` from its own tile data
- `presentation.js` — owns a `WorldView`; `applyEvents(events[])` drives all visual updates; detects structural changes (building added/removed) to trigger neighbor visual refresh
- `displayTile.js` / `displayObject.js` — Three.js `Group` subclasses; `refreshView(tileView, worldView)` takes presentation-side data only
- `buildings/` — display building classes; `refreshView(tileView, worldView)` reads from the tile/building snapshot, never from the sim world

### Orchestration
`game.js` wires everything: creates renderer, simulation, and presentation; subscribes to simulation events; handles input by sending commands (`placeBuilding`, `bulldoze`) to the simulation. `ui.js` takes plain data objects — never references `game.simulation.world`.

## CQRS Event Flow

```
Simulation tick / command
  → emits events[]  (WorldSnapshot | TileChanged | StatsChanged)
    → presentation.applyEvents(events)
      → worldView.apply(events)       (update read model)
      → refresh changed DisplayTiles  (using worldView data only)
    → ui.updateTitleBar(name, stats)
    → ui.updateInfoPanel(worldView.getTile(...))
```

**Commands** (presentation → simulation, via `game.js`):
- `simulation.placeBuilding(x, y, type)`
- `simulation.bulldoze(x, y)`

## Key Patterns

**Dirty flag**: Each tick, `simulation.js` sets `tile.updated = true` on changed tiles. After the tick, `TileChanged` events are emitted for updated tiles. `#cleanWorld()` resets flags at tick start.

**SimModule composition**: `BuildingManager` selects which `SimModule` subclasses to attach based on building type. Modules implement `simulate(dt)` and `dispose()`.

**Neighbor visual refresh**: When a `TileChanged` event changes building presence (structural change), `presentation.js` also visually refreshes the 4 cardinal neighbors — so roads and power lines update their connectivity style.

**Raycasting for input**: Three.js `Raycaster` resolves mouse position to a `DisplayObject`. Each mesh stores a back-reference via `userData`. `game.js` maps the result to a `worldView.getTile(x, y)` for UI display.

**Asset cloning**: `AssetManager` preloads all GLTF models once, then clones per-instance with unique materials to support per-building highlight state.

## Configuration

`src/scripts/config.js` — all tunable simulation parameters in one place (development rates, job counts, citizen behavior, vehicle settings).

## File Layout

```
src/
  index.html
  scripts/
    game.js          # Orchestrator — wires sim events to presentation commands
    config.js        # Simulation constants
    ui.js            # HTML overlay; takes plain data, never sim world refs
    input.js
    camera.js
    assets/
      assetManager.js
      models.js
    model/           # Sim-side data only — not read by presentation
      world.js
      tile.js
      citizen.js
      buildings/
        building.js
        buildingType.js    # Shared string constants (OK to import anywhere)
        buildingStatus.js  # Shared string constants
        zones/         residential.js, commercial.js, industrial.js
        power/         powerPlant.js, powerLine.js, powerConsumer.js
        transportation/ road.js
    sim/             # Logic — no Three.js
      simulation.js
      worldEvents.js   # Event factories (CQRS boundary)
      tileTools.js
      citizenManager.js
      buildings/
        buildingManager.js
        modules/     simModule.js, development.js, residents.js,
                     jobs.js, commerce.js, roadAccess.js
      services/
        simService.js
        power.js
    scene/           # Three.js — no sim world references
      worldView.js     # Presentation read model
      presentation.js
      displayObject.js
      displayTile.js
      vehicles/      vehicle.js, vehicleGraph.js, vehicleGraphNode.js
      buildings/     buildingFactory.js, building.js, zone/zone.js,
                     transportation/road.js, power/powerPlant.js, powerLine.js
  public/
    models/          GLTF 3D assets
    textures/
    statusIcons/
    icons/
    fonts/
```

## Vite Config Notes

- Root is `./src`, public dir is `./src/public`, output is `./dist`
- Base path is `/simcity-threejs-clone/` for GitHub Pages deployment
- ES modules throughout (`"type": "module"` in package.json)

## Debugging Notes

- If we end up with left over server we can kill it with
```
! powershell -Command "Stop-Process -Id (Get-NetTCPConnection -LocalPort 3001).OwningProcess -Force"
```