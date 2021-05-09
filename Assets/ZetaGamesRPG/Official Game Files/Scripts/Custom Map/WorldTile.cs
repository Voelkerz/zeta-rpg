using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ZetaGames.RPG {
    [System.Serializable]
    public class WorldTile {

        // Specific Tile Data
        public bool occupied = false;
        public ResourceCategory occupiedCategory = ResourceCategory.None;
        public ResourceType occupiedType = ResourceType.None;
        public string occupiedStatus = ZetaUtilities.OCCUPIED_NONE;
        public string lockTag = ZetaUtilities.TAG_NONE;
        public BaseObjectData tileObjectData = null;
        private GameObject tileObject = null;
        
        // Global Tile Data
        public float pathingPenalty;
        public bool walkable;
        public string terrainType;
        public float speedPercent;
        public int elevation;

        // Tilemap Data
        public Dictionary<int, string> tileSprites = new Dictionary<int, string>();
        public string ruleTileName = "none";
        
        // World Grid Data
        public int x;
        public int y;

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

        public GameObject GetTileObject() {
            return tileObject;
        }
        public void SetTileObject(GameObject tileObject) {
            this.tileObject = tileObject;
        }

        public bool HasTileObject() {
            return tileObject != null;
        }
    }
}

