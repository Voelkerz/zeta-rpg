using UnityEngine;
using UnityEngine.AI;

namespace ZetaGames {
    static class ZetaUtilities {
        // GameObject Tag Constants
        public static readonly string TAG_CULLED = "Culled";
        public static readonly string TAG_UNCULLED = "Unculled";
        public static readonly string TAG_CULLLOCKED = "CullLocked";

        // Tile Occupied Type Constants
        public static readonly string OCCUPIED_NONE = "_None";
        public static readonly string OCCUPIED_NODE = "_Node";
        public static readonly string OCCUPIED_NODE_FULL = "_Node_Full";
        public static readonly string OCCUPIED_NODE_DEPLETED = "_Node_Depleted";
        public static readonly string OCCUPIED_NODE_ADJACENT = "_Node_Adjacent";
        public static readonly string OCCUPIED_ITEMPICKUP = "_ItemPickup";

        // Terrain Constants
        public static readonly string TERRAIN_GRASS = "Grass";
        public static readonly string TERRAIN_WATER = "Water";
        public static readonly string TERRAIN_DIRT = "Dirt";
    }
}

