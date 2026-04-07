import { EventType } from '../../shared/eventTypes.js';
export { EventType };

/**
 * Full world state snapshot, emitted once when a subscriber registers.
 */
export function worldSnapshotEvent(world) {
    const tiles = [];
    for (let x = 0; x < world.size; x++) {
        for (let y = 0; y < world.size; y++) {
            tiles.push(tileSnapshot(world.tiles[x][y]));
        }
    }
    return {
        type: EventType.WorldSnapshot,
        name: world.name,
        size: world.size,
        simTime: world.simTime,
        population: world.citizens.length,
        demand: { ...world.stats.demand },
        tiles,
    };
}

/**
 * Emitted for each tile whose state changed during a tick or command.
 */
export function tileChangedEvent(tile) {
    return {
        type: EventType.TileChanged,
        ...tileSnapshot(tile),
    };
}

/**
 * Emitted once per tick with world-level statistics.
 */
export function statsChangedEvent(world) {
    return {
        type: EventType.StatsChanged,
        simTime: world.simTime,
        population: world.citizens.length,
        demand: { ...world.stats.demand },
    };
}

function tileSnapshot(tile) {
    return {
        x: tile.x,
        y: tile.y,
        terrain: tile.terrain,
        building: tile.building ? buildingSnapshot(tile.building) : null,
    };
}

function buildingSnapshot(b) {
    const snap = {
        type: b.type,
        status: b.status,
        hideTerrain: b.hideTerrain,
        needsRoadAccess: b.needsRoadAccess,
        hasRoadAccess: b.hasRoadAccess,
    };
    if (b.name) {
        snap.name = b.name;
    }
    if (b.development) {
        snap.development = {
            state: b.development.state,
            level: b.development.level,
        };
    }
    if (b.power) {
        snap.power = {
            supplied: b.power.supplied,
            required: b.power.required,
        };
    }
    if (b.residents !== undefined) {
        snap.residents = b.residents.map(r => ({ name: r.name, age: r.age, state: r.state }));
        snap.maxResidents = b.maxResidents;
    }
    if (b.workers !== undefined) {
        snap.workers = b.workers.map(w => ({ name: w.name, age: w.age, state: w.state }));
        snap.maxWorkers = b.maxWorkers;
    }
    if (b.commerce !== undefined) {
        snap.commerce = { capacity: b.commerce.capacity };
    }
    return snap;
}
