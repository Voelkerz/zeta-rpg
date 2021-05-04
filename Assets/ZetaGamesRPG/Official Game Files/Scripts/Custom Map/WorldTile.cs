using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ZetaGames.RPG {
    [System.Serializable]
    public class WorldTile {

        // Specific Tile Data
        public bool occupied = false;
        public string occupiedCategory = ResourceCategory.None.ToString();
        public string occupiedType = ResourceType.None.ToString();
        public string occupiedStatus = ZetaUtilities.OCCUPIED_NONE;
        private TileObjectPool tileObjectPool = null;
        private GameObject tileObject = null;
        private Vector3 tileObjectPos;

        // Global Tile Data
        public float pathPenalty;
        public bool walkable;
        public string terrainType;
        public float speedPercent;
        public bool animated;

        // Tilemap Data
        public List<string> spriteNames = new List<string>();
        public int tilemap;
        public bool loaded = false;
        public string[] animSpriteNames;
        public float animMinSpeed;
        public float animMaxSpeed;
        
        // World Grid Data
        public int x;
        public int y;

        // Chunk Grid Data
        public int chunkX;
        public int chunkY;

        // Constructor
        public WorldTile() { }

        public WorldTile(int x, int y) {
            this.x = x;
            this.y = y;
        }

        public void SetTileObjectPool(TileObjectPool tileObjectPool) {
            this.tileObjectPool = tileObjectPool;
        }

        public void SetTileObject(GameObject tileObject) {
            this.tileObject = tileObject;
        }

        public TileObjectPool GetTileObjectPool() {
            return tileObjectPool;
        }

        public GameObject GetTileObject() {
            return tileObject;
        }

        public bool HasTileObjectPool() {
            return tileObjectPool != null;
        }

        public bool HasTileObject() {
            return tileObject != null;
        }

        public void InstantiatePooledObject() {
            //Debug.Log("WorldTile.InstantiatePooledObject(): Pooling: " + occupiedType + occupiedStatus);
            tileObject = tileObjectPool.GetPooledObject(occupiedType + occupiedCategory + occupiedStatus);
            if (tileObject != null) {
                tileObject.tag = ZetaUtilities.TAG_UNCULLED;
                tileObjectPos.x = x;
                tileObjectPos.y = y;
                tileObject.transform.position = tileObjectPos;
                tileObject.transform.rotation = Quaternion.identity;
            }
        }

        public void RemovePooledObject() {
            if (tileObject != null) {
                tileObject.tag = ZetaUtilities.TAG_CULLED;
                tileObject = null;
            }
        }
    }
}

