using Pathfinding;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    public class HarvestableResource : MonoBehaviour {

        public ResourceNodeData resourceData;
        private int currentHitPoints;

        private void Start() {
            currentHitPoints = resourceData.maxHitPoints;
        }

        public bool ToolHit(int hitAmount) {
            if (currentHitPoints > 0) {
                currentHitPoints -= hitAmount;
                return true;
            } else {
                return false;
            }
        }

        public int GetHealth() {
            return currentHitPoints;
        }

        public void RecycleAndSpawnLoot() {
            List<WorldTile> tilesToUpdate = new List<WorldTile>();

            // Adjust tile data
            WorldTile curTile = MapManager.Instance.GetWorldTileGrid().GetGridObject(transform.position);
            tilesToUpdate.Add(curTile);
            curTile.SetTileObject(Instantiate(resourceData.resourceLoot, gameObject.transform.position + new Vector3(0.5f, 0.5f), Quaternion.identity));
            curTile.SetTileObjectPool(null);
            curTile.occupied = true;
            curTile.occupiedType = resourceData.resourceType.ToString() + "_drop";
            curTile.walkable = true;

            // Adjust adjacent tile data
            foreach (Vector3Int adjGridPos in resourceData.adjacentGridOccupation) {
                WorldTile adjTile = MapManager.Instance.GetWorldTileGrid().GetGridObject(curTile.x + adjGridPos.x, curTile.y + adjGridPos.y);
                tilesToUpdate.Add(adjTile);
                adjTile.occupied = false;
                adjTile.occupiedType = "none";
                adjTile.walkable = true;
            }

            // Adjust pathfinding grid
            AstarPath.active.AddWorkItem(new AstarWorkItem(() => {}, force => {
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

            // Reset resource and send back into reusable object pool
            gameObject.transform.position = new Vector3Int(0, 0, 0);
            gameObject.tag = "Culled";
            currentHitPoints = resourceData.maxHitPoints;
        }
    }
}

