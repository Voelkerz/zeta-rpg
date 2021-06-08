using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    [System.Serializable]
    public class WorldTile {

        // Specific Tile Data
        public bool occupied = false;
        public string occupiedStatus = ZetaUtilities.OCCUPIED_NONE;
        public TilemapObstacle tileObstacle = null;
        public bool hasParent;
        public int lockTag = -1;
        public int lootAvailable;
        
        // Global Tile Data
        public uint pathPenalty;
        public bool walkable;
        public string terrainType;
        public float speedPercent;
        public int elevation = 0;

        // Tilemap Data
        public Dictionary<int, string> tileSprites = new Dictionary<int, string>();
        public string ruleTileName = "none";
        
        // World Grid Data
        public int x;
        public int y;

        // World Grid Data of Parent Tile
        public int parentX;
        public int parentY;

        // Region Grid
        public int regionX;
        public int regionY;

        // Constructor
        public WorldTile(int x, int y) {
            this.x = x;
            this.y = y;
        }

        public Vector3 GetWorldPosition() {
            return new Vector3(x, y);
        }

        public Vector3Int GetWorldPositionInt() {
            return new Vector3Int(x, y, 0);
        }

        public WorldTile GetParentTile() {
            return MapManager.Instance.GetWorldTileGrid().GetGridObject(parentX, parentY);
        }

        public List<WorldTile> SetParentTileObstacle(TilemapObstacle tileObstacle, int tilemapLayer, string parentStatus, string childStatus) {
            List<WorldTile> updatedTiles = new List<WorldTile>();
            // If this tile already has an obstacle, remove it first
            if (this.tileObstacle != null) {
                //Debug.Log("WorldTile.SetTileObstacle(): Replacing an already existing tile obstacle.");
                updatedTiles.AddRange(RemoveTileObstacle(tilemapLayer));
            }

            // Set the additional child grid occupancy
            if (tileObstacle != null) {
                if (tileObstacle.additionalGridOccupation != null && tileObstacle.additionalGridOccupation.Count > 0) {
                    foreach (Vector3Int adjTilePos in tileObstacle.additionalGridOccupation) {
                        WorldTile otherTile = MapManager.Instance.GetWorldTileGrid().GetGridObject(x + adjTilePos.x, y + adjTilePos.y);

                        if (otherTile != null) {
                            //Debug.Log("OtherTile: (" + otherTile.x + ", " + otherTile.y + ")");
                        } else {
                            Debug.LogError("OtherTile is null");
                        }

                        // If this tile already has an obstacle, remove it first
                        if (otherTile.tileObstacle != null) {
                            //Debug.Log("WorldTile.SetTileObstacle(): Replacing an already existing tile obstacle found from adjacent tile.");
                            updatedTiles.AddRange(otherTile.RemoveTileObstacle(tilemapLayer));
                        }

                        otherTile.tileObstacle = tileObstacle;
                        otherTile.occupiedStatus = (childStatus != null) ? childStatus : ZetaUtilities.OCCUPIED_OBSTACLE_ADJACENT;
                        otherTile.walkable = tileObstacle.walkable;
                        otherTile.occupied = true;
                        otherTile.hasParent = true;
                        otherTile.parentX = x;
                        otherTile.parentY = y;

                        updatedTiles.Add(otherTile);
                    }
                }

                this.tileObstacle = tileObstacle;
                occupiedStatus = parentStatus;
                walkable = (tileObstacle != null) ? tileObstacle.walkable : true;
                occupied = true;
                hasParent = false;

                updatedTiles.Add(this);

                // Set obstacle sprites on parent tile
                MapManager.Instance.SetMapSpriteTile(this, tilemapLayer + elevation, tileObstacle.spriteName);
                MapManager.Instance.SetMapSpriteTile(this, tilemapLayer + elevation + 1, tileObstacle.spriteShadowName);
            } else {
                MapManager.Instance.tileMapList[tilemapLayer + elevation].SetTile(GetWorldPositionInt(), null);
                MapManager.Instance.tileMapList[tilemapLayer + elevation + 1].SetTile(GetWorldPositionInt(), null);
            }

            // return list of updated tiles
            return updatedTiles;
        }

        public List<WorldTile> RemoveTileObstacle(int tilemapLayer) {
            List<WorldTile> updatedTiles = new List<WorldTile>();
            // If this is a child obstacle tile, then set to the parent. Otherwise, this is the parent.
            WorldTile parentTile = hasParent ? GetParentTile() : this;

            if (parentTile.tileObstacle != null) {
                // Reset the additional grid occupied tiles if there are some
                if (parentTile.tileObstacle.additionalGridOccupation != null && parentTile.tileObstacle.additionalGridOccupation.Count > 0) {
                    foreach (Vector3Int adjTilePos in parentTile.tileObstacle.additionalGridOccupation) {
                        WorldTile otherTile = MapManager.Instance.GetWorldTileGrid().GetGridObject(parentTile.x + adjTilePos.x, parentTile.y + adjTilePos.y);
                        
                        if (otherTile != null) {
                            // Reset tile
                            otherTile.ResetTile();

                            updatedTiles.Add(otherTile);
                        } else {
                            Debug.LogWarning("WorldTile.RemoveTileObstacle(): Additional grid tile to reset is null.");
                        }
                    }
                }

                //Debug.Log("Nullifying parent tile at: (" + parentTile.x + ", " + parentTile.y + ")");

                // Nullify obstacle sprites on parent tile
                MapManager.Instance.tileMapList[tilemapLayer + elevation].SetTile(parentTile.GetWorldPositionInt(), null);
                MapManager.Instance.tileMapList[tilemapLayer + elevation + 1].SetTile(parentTile.GetWorldPositionInt(), null);

                // Reset tile
                parentTile.ResetTile();

                updatedTiles.Add(parentTile);
            } else {
                Debug.LogWarning("This parent obstacle tile does not have a registered tile obstacle object.");
            }

            // return list of updated tiles
            return updatedTiles;
        }

        public void ResetTile() {
            occupied = false;
            occupiedStatus = ZetaUtilities.OCCUPIED_NONE;
            lockTag = -1;
            tileObstacle = null;
            walkable = true;
            lootAvailable = 0;
            hasParent = false;
        }
    }
}

