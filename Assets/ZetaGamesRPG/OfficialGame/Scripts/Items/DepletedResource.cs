using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    public class DepletedResource : MonoBehaviour {

        public void DigUp() {
            /*
            List<WorldTile> tilesToUpdate = new List<WorldTile>();

            // Spawn depleted node in current tile
            WorldTile curTile = MapManager.Instance.GetWorldTileGrid().GetGridObject(transform.position);
            curTile.occupiedStatus = resourceData.resourceType.ToString() + ZetaUtilities.OCCUPIED_NODE_DEPLETED;
            curTile.InstantiatePooledObject();
            tilesToUpdate.Add(curTile);

            // Adjust adjacent tile data
            foreach (Vector3Int adjGridPos in resourceData.adjacentGridOccupation) {
                WorldTile adjTile = MapManager.Instance.GetWorldTileGrid().GetGridObject(curTile.x + adjGridPos.x, curTile.y + adjGridPos.y);
                tilesToUpdate.Add(adjTile);
                adjTile.occupied = false;
                adjTile.occupiedStatus = "none";
                adjTile.walkable = true;
            }

            // Adjust pathfinding grid
            AstarPath.active.AddWorkItem(new AstarWorkItem(() => { }, force => {
                // Called each frame until returned true
                var grid = AstarPath.active.data.gridGraph;

                // Update all tiles in list
                foreach (WorldTile tile in tilesToUpdate) {
                    Vector3 tilePos = MapManager.Instance.GetWorldTileGrid().GetWorldPosition(tile.x, tile.y);
                    grid.GetNode((int)tilePos.x, (int)tilePos.y).Walkable = true;
                    grid.CalculateConnectionsForCellAndNeighbours((int)tilePos.x, (int)tilePos.y);
                }

                return true;
            }));
            // TODO: Change layer or Z-level instead of moving to -50,-50?
            // Reset resource and send back into reusable object pool
            gameObject.transform.position = new Vector3Int(-50, -50, 0);
            gameObject.tag = "Culled";
            currentHitPoints = resourceData.maxHitPoints;
            */
        }
    }
}
