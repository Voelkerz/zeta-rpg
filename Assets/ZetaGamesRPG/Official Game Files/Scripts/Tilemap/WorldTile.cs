using System.Collections;
using System.Collections.Generic;

namespace ZetaGames.RPG {
    public class WorldTile {

        // Specific Tile Data
        public bool occupied;
        
        // Global Tile Data
        public float movementCost;
        public bool walkable;
        public string type;

        // Grid Data
        private ZGrid<WorldTile> grid;
        private int x;
        private int y;

        public WorldTile(ZGrid<WorldTile> grid, int x, int y) {
            this.grid = grid;
            this.x = x;
            this.y = y;
        }
    }
}

