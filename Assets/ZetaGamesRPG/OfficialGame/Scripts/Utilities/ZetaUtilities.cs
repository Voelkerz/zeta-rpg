using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
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
        public static readonly string OCCUPIED_NONE = "None";
        public static readonly string OCCUPIED_NODE = "_Node";
        public static readonly string OCCUPIED_NODE_FULL = "_Node_Full";
        public static readonly string OCCUPIED_NODE_DEPLETED = "_Node_Depleted";
        public static readonly string OCCUPIED_NODE_ADJACENT = "_Node_Adjacent";
        public static readonly string OCCUPIED_OBSTACLE_ADJACENT = "Adjacent";
        public static readonly string OCCUPIED_ITEMPICKUP = "_ItemPickup";
        public static readonly string OCCUPIED_STRUCTURE = "Structure";
        public static readonly string OCCUPIED_STRUCTURE_BUILDSITE = "Structure Build Site";
        public static readonly string OCCUPIED_STRUCTURE_WALL = "Structure Wall";
        public static readonly string OCCUPIED_STRUCTURE_DOOR = "Structure Door";
        public static readonly string OCCUPIED_SETTLEMENT_WALL_WEST = "Settlement West Wall";
        public static readonly string OCCUPIED_SETTLEMENT_WALL_NORTH = "Settlement North Wall";
        public static readonly string OCCUPIED_SETTLEMENT_WALL_EAST = "Settlement East Wall";
        public static readonly string OCCUPIED_SETTLEMENT_WALL_SOUTH = "Settlement South Wall";
        public static readonly string OCCUPIED_SETTLEMENT_WALL_CORNER = "Settlement Wall Corner";
        public static readonly string OCCUPIED_SETTLEMENT_WALL_ADJACENT = "Settlement Wall Adjacent";
        public static readonly string OCCUPIED_SETTLEMENT_ENTRANCE = "Settlement Entrance";
        public static readonly string OCCUPIED_SETTLEMENT_ROAD_MAIN = "Settlement Main Road";
        public static readonly string OCCUPIED_SETTLEMENT_ROAD_SIDE = "Settlement Side Road";

        // Terrain Constants
        public static readonly string TERRAIN_GRASS = "Grass";
        public static readonly string TERRAIN_WATER = "Water";
        public static readonly string TERRAIN_DIRT = "Dirt";
        public static readonly string TERRAIN_SETTLEMENT_ROAD = "Dirt Road";

        // NPC Ownership Constants
        public static readonly string OWNED_HOME = "House";
        public static readonly string OWNED_BUSINESS = "Business";
        public static readonly string OWNED_CONTAINER = "Container";

        public static IEnumerator WaitForSeconds(float seconds) {
            yield return new WaitForSecondsRealtime(seconds);
        }

        public static void UpdateSingleAstarGraphNode(WorldTile tile) {
            AstarPath.active.AddWorkItem(new AstarWorkItem(ctx => {
                GridGraph gg = AstarPath.active.data.gridGraph;
                GridNodeBase node = gg.GetNode(tile.x, tile.y);
                node.Walkable = tile.walkable;
                gg.CalculateConnectionsForCellAndNeighbours(tile.x, tile.y);
            }));
        }

        public static void UpdateMultipleAstarGraphNodes(List<WorldTile> tileList) {
            AstarPath.active.AddWorkItem(new AstarWorkItem(() => {
                
            },
        force => {
            GridGraph gg = AstarPath.active.data.gridGraph;

            for (int i = 0; i < tileList.Count; i++) {
                WorldTile tile = tileList[i];
                GridNodeBase node = gg.GetNode(tile.x, tile.y);
                node.Walkable = tile.walkable;
                gg.CalculateConnectionsForCellAndNeighbours(tile.x, tile.y);
            }

            return true;
        }));
        }
    }
}

