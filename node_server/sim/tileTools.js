import {Tile} from "../model/tile.js";

/** Returns the title at the coordinates. If the coordinates
 * are out of bounds, then `null` is returned.
 * @params {World} world
 * @param {number} x The x-coordinate of the tile
 * @param {number} y The y-coordinate of the tile
 * @returns {Tile | null}
 */
export function getTile(world, x, y) {
    if (x === undefined || y === undefined ||
        x < 0 || y < 0 ||
        x >= world.size || y >= world.size) {
        return null;
    } else {
        return world.tiles[x][y];
    }
}

export class NeighborMatches {
    top = false;
    right = false;
    left = false;
    bottom = false;
}
/**
 *
 * @param {World} world
 * @param {int} x
 * @param {int} y
 * @param {BuildingType} type
 * @returns {NeighborMatches}
 */
export function getMatchingNeighbors(world, x, y, type) {
    let matches = new NeighborMatches();
    matches.top = (getTile(world, x, y - 1)?.building?.type === type) ?? false;
    matches.bottom = (getTile(world, x, y + 1)?.building?.type === type) ?? false;
    matches.left = (getTile(world, x - 1, y)?.building?.type === type) ?? false;
    matches.right = (getTile(world, x + 1, y)?.building?.type === type) ?? false;
    return matches
}

/**
 * Finds the first tile where the criteria are true
 * @param {World} world
 * @param {number} x The x-coordinate of the tile
 * @param {number} y The y-coordinate of the tile
 * @param {(Tile) => (boolean)} filter This function is called on each
 * tile in the search field until `filter` returns true, or there are
 * no more tiles left to search.
 * @param {number} maxDistance The maximum distance to search from the starting tile
 * @returns {Tile | null} The first tile matching `criteria`, otherwise `null`
 */
export function findTile(world, x,y, filter, maxDistance) {
    const startTile = getTile(world, x, y);
    const visited = new Set();
    const tilesToSearch = [];

    // Initialize our search with the starting tile
    tilesToSearch.push(startTile);

    while (tilesToSearch.length > 0) {
        const tile = tilesToSearch.shift();

        // Has this tile been visited? If so, ignore it and move on
        if (visited.has(tile.id)) {
            continue;
        } else {
            visited.add(tile.id);
        }
        // Check if tile is outside the search bounds
        const distance = Math.abs(startTile.x - tile.x) + Math.abs(startTile.y - tile.y);
        if (distance > maxDistance) continue;

        // Add this tiles neighbor's to the search list
        tilesToSearch.push(... getTileNeighbors(world, tile.x, tile.y));

        // If this tile passes the criteria
        if (filter(tile)) {
            return tile;
        }
    }

    return null;
}

/**
 * Finds and returns the neighbors of this tile
 * @param {World} world
 * @param {number} x The x-coordinate of the tile
 * @param {number} y The y-coordinate of the tile
 */
function getTileNeighbors(world, x, y) {
    const neighbors = [];

    if (x > 0) {
        neighbors.push(getTile(world, x - 1, y));
    }
    if (x < world.size - 1) {
        neighbors.push(getTile(world, x + 1, y));
    }
    if (y > 0) {
        neighbors.push(getTile(world, x, y - 1));
    }
    if (y < world.size - 1) {
        neighbors.push(getTile(world, x, y + 1));
    }

    return neighbors;
}
