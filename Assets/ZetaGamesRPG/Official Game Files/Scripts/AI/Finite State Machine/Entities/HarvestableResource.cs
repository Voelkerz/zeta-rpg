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
            // Spawn depleted node in current tile
            WorldTile curTile = MapManager.Instance.GetWorldTileGrid().GetGridObject(transform.position);
            curTile.occupiedStatus = resourceData.resourceType.ToString() + ZetaUtilities.OCCUPIED_NODE_DEPLETED;
            curTile.occupiedType = resourceData.type.ToString();
            curTile.occupied = true;
            curTile.InstantiatePooledObject();
            Vector3 curTileWorldPos = MapManager.Instance.GetWorldTileGrid().GetWorldPosition(curTile.x, curTile.y);

            // Spawn loot in the adjacent tiles around node (4x3 grid around node center)
            int numLoot = resourceData.lootAmount;
            List<WorldTile> possibleLootPositions = new List<WorldTile>();

            // create list of possible locations for loot to spawn
            for (int x = 0; x < 4; x++) {
                for (int y = 0; y < 3; y++) {
                    // check grid bounds
                    if (MapManager.Instance.GetWorldTileGrid().IsWithinGridBounds((int)curTileWorldPos.x + (x - 1), (int)curTileWorldPos.y + (y - 1))) {
                        WorldTile adjTile = MapManager.Instance.GetWorldTileGrid().GetGridObject((int)curTileWorldPos.x + (x - 1), (int)curTileWorldPos.y + (y - 1));
                        
                        // If tile is not occupied and is walkable
                        if (!adjTile.occupied && adjTile.walkable) {
                            possibleLootPositions.Add(adjTile);
                        }
                    }
                }
            }

            // Spawn max number of loot on random viable adjacent tiles
            for (int i = 0; i < numLoot; i++) {
                WorldTile chosenTile = possibleLootPositions[Random.Range(0, possibleLootPositions.Count - 1)];
                possibleLootPositions.Remove(chosenTile);
                chosenTile.SetTileObject(Instantiate(resourceData.lootPrefab, MapManager.Instance.GetWorldTileGrid().GetWorldPosition(chosenTile.x, chosenTile.y) + new Vector3(0.5f, 0.5f), Quaternion.identity));

                // Adjust tile data
                chosenTile.occupiedStatus = resourceData.resourceType.ToString() + ZetaUtilities.OCCUPIED_ITEMPICKUP;
                chosenTile.occupiedType = resourceData.type.ToString();
                chosenTile.occupied = true;
            }

            // TODO: Change layer or Z-level instead of moving to -50,-50?
            // Reset resource and send back into reusable object pool
            gameObject.transform.position = new Vector3Int(-50, -50, 0);
            gameObject.tag = "Culled";
            currentHitPoints = resourceData.maxHitPoints;
        }
    }
}

