using UnityEngine;
using UnityEngine.Tilemaps;

namespace ZetaGames.RPG {
    [System.Serializable]
    public class WorldTile {

        // Specific Tile Data
        public bool occupied = false;
        public string occupiedType = "none";
        private ObjectPool tileObjectPool = null;
        private GameObject tileObject = null;
        private Vector3 tileObjectPos;

        // Global Tile Data
        public float pathPenalty;
        public bool walkable;
        public string type;
        public float speedPercent;
        public bool animated;

        // Tilemap Data
        public string spriteName;
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

        public void SetTileObjectPool(ObjectPool tileObjectPool) {
            this.tileObjectPool = tileObjectPool;
        }

        public void SetTileObject(GameObject tileObject) {
            this.tileObject = tileObject;
        }

        public ObjectPool GetTileObjectPool() {
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
            tileObject = tileObjectPool.GetPooledObject();
            if (tileObject != null) {
                tileObject.tag = "Unculled";
                tileObjectPos.x = x;
                tileObjectPos.y = y;
                tileObject.transform.position = tileObjectPos;
                tileObject.transform.rotation = Quaternion.identity;
            }
        }

        public void RemovePooledObject() {
            if (tileObject != null) {
                tileObject.tag = "Culled";
                tileObject = null;
            }
        }
    }
}

