namespace ZetaGames {
    static class ZetaUtilities {
        // Object Pooling Constants
        public static readonly string TAG_CULLED = "Culled";
        public static readonly string TAG_UNCULLED = "Unculled";
        public static readonly string TAG_CULLLOCKED = "CullLocked";
        public static readonly string TAG_NONE = "None";

        // Memory Key Constants
        public static readonly string MEMORY_LOCATION_HOME = "Home";
        public static readonly string MEMORY_LOCATION_SETTLEMENT = "Settlement";
        public static readonly string MEMORY_RESOURCE_DROP = "ResourceDrop";
        public static readonly string MEMORY_RESOURCE_NODE = "ResourceNode";

        // Tile Occupied Type Constants
        public static readonly string OCCUPIED_NONE = "_None";
        public static readonly string OCCUPIED_NODE = "_Node";
        public static readonly string OCCUPIED_NODE_FULL = "_Node_Full";
        public static readonly string OCCUPIED_NODE_DEPLETED = "_Node_Depleted";
        public static readonly string OCCUPIED_NODE_ADJACENT = "_Node_Adjacent";
        public static readonly string OCCUPIED_ITEMPICKUP = "_ItemPickup";
        public static readonly string OCCUPIED_STRUCTURE = "Structure";
        public static readonly string OCCUPIED_STRUCTURE_WALL = "Structure_Wall";
        public static readonly string OCCUPIED_STRUCTURE_DOOR = "Structure_Door";

        // Terrain Constants
        public static readonly string TERRAIN_GRASS = "Grass";
        public static readonly string TERRAIN_WATER = "Water";
        public static readonly string TERRAIN_DIRT = "Dirt";

        // NPC Ownership Constants
        public static readonly string OWNED_HOME = "House";
        public static readonly string OWNED_BUSINESS = "Business";
        public static readonly string OWNED_CONTAINER = "Container";
    }
}

