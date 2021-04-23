using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ZetaGames.RPG {
    public class MapManager : MonoBehaviour {

        [SerializeField] private List<Tilemap> mapList;
        [SerializeField] private List<GlobalTileData> globalTileDataList;
        [SerializeField] private int mapWidth = 47;
        [SerializeField] private int mapHeight = 47;
        [SerializeField] private float cellSize = 1;
        [SerializeField] private Vector3 originPosition = new Vector3(-22f, -24f);
        [HideInInspector] public Dictionary<Vector3Int, WorldTile> worldTileDictionary;
        private Dictionary<TileBase, GlobalTileData> globalTileDataDictionary;
        private ZGrid<WorldTile> worldTileGrid;

        private void Start() {
            // Initialize
            worldTileGrid = new ZGrid<WorldTile>(mapWidth, mapHeight, cellSize, originPosition, (ZGrid<WorldTile> g, int x, int y) => new WorldTile(g, x, y));
            globalTileDataDictionary = new Dictionary<TileBase, GlobalTileData>();
            worldTileDictionary = new Dictionary<Vector3Int, WorldTile>();

            // Create dictionary of all BaseTiles and their global data
            foreach (GlobalTileData tileData in globalTileDataList) {
                foreach (TileBase tile in tileData.tiles) {
                    if (!globalTileDataDictionary.ContainsKey(tile)) {
                        globalTileDataDictionary.Add(tile, tileData);
                    }
                }
            }

            // Create master world tile dictionary. Will hold individual data on every tile in the game.
            for (int gridX = 0; gridX < worldTileGrid.GetWidth(); gridX++) {
                for (int gridY = 0; gridY < worldTileGrid.GetHeight(); gridY++) {
                    Vector3Int gridPos = new Vector3Int(gridX, gridY, 0);

                    foreach (Tilemap tilemap in mapList) {
                        WorldTile worldTile = worldTileGrid.GetGridObject(gridPos.x, gridPos.y);

                        TileBase tile = tilemap.GetTile(new Vector3Int((int)worldTileGrid.GetWorldPosition(gridPos.x, gridPos.y).x, (int)worldTileGrid.GetWorldPosition(gridPos.x, gridPos.y).y, 0));

                        if (tile != null) {
                            if (globalTileDataDictionary.TryGetValue(tile, out GlobalTileData globalTileData)) {
                                worldTile.movementCost = globalTileData.movementCost;
                                worldTile.walkable = globalTileData.walkable;
                                worldTile.type = globalTileData.type;

                                if (worldTileDictionary.ContainsKey(gridPos)) {
                                    //Debug.Log("Overwriting world tile with higher priority tile. WorldTile type: " + worldTile.type);
                                    worldTileDictionary[gridPos] = worldTile;
                                } else {
                                    worldTileDictionary.Add(gridPos, worldTile);
                                    //Debug.Log("Adding new world tile. WorldTile type: " + worldTile.type);
                                }
                            } else {
                                Debug.Log("BaseTile not found in global tile dictionary: " + tile.name);
                            }
                        } else {
                            //Debug.Log("Null tile on Tilemap");
                        }      
                    }
                }
            }

            // Update Astar pathfinding graph to include world tile info
            AstarPath.active.AddWorkItem(() => {
                var gg = AstarPath.active.data.gridGraph;
                WorldTile worldTile;

                gg.GetNodes(node => {
                    // Find world tile based on current node position
                    worldTileGrid.GetXY((Vector3)node.position, out int x, out int y);
                    Vector3Int worldTilePos = new Vector3Int(x, y, 0);

                    if (worldTileDictionary.TryGetValue(worldTilePos, out worldTile)) {
                        // walkability
                        if (!worldTile.walkable) {
                            node.Walkable = false;
                        }

                        // movement cost (only affects pathfinding here, not character speed)
                        node.Penalty = (uint)(1000 * worldTile.movementCost);

                        // terrain type sets node tag
                       
                    } else {
                        Debug.Log("Node does not align with world tile");
                    }
                });
            });

            
        }

        private void Update() {
            /*
            if (Input.GetMouseButtonDown(0)) {
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                worldTileGrid.GetXY(mouseWorldPos, out int x, out int y);
                Vector3Int mouseGridPos = new Vector3Int(x, y, 0);

                WorldTile clickedTile = worldTileDictionary[mouseGridPos];

                Debug.Log("Type: " + clickedTile.type + " || Walkable: " + clickedTile.walkable + " || Move cost: " + clickedTile.movementCost);
            }
            */
        }

        /*
        public float GetTileMovementCost(Vector2 worldPosition) {
            Vector3Int gridPosition = map.WorldToCell(worldPosition);
            TileBase tile = map.GetTile(gridPosition);

            if (tile == null) {
                return 20;
            }

            return dataFromTiles[tile].movementCost;
        }
        */
    }
}
