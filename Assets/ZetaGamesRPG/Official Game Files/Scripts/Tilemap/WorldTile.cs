using UnityEngine;

namespace ZetaGames.RPG {
    [System.Serializable]
    public class WorldTile {

        // Specific Tile Data
        public bool occupied = false;
        public string occupiedType = "none";
        
        // Global Tile Data
        public float pathPenalty;
        public bool walkable;
        public string type;
        public float speedPercent;

        // Tilemap Data
        public string spriteName;
        public string tilemap;

        // Grid Data
        private ZGrid<WorldTile> grid;
        public int x;
        public int y;

        // Constructor
        public WorldTile(ZGrid<WorldTile> grid, int x, int y) {
            this.grid = grid;
            this.x = x;
            this.y = y;
        }
    }
}

