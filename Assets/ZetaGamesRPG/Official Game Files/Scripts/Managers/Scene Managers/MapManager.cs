using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using UnityEngine.Tilemaps;
using Newtonsoft.Json;
using System.Collections;

namespace ZetaGames.RPG {
    public class MapManager : MonoBehaviour {

        [SerializeField] private List<Tilemap> mapList;
        [SerializeField] private List<GlobalTileData> globalTileDataList;
        [SerializeField] private int mapWidth = 50;
        [SerializeField] private int mapHeight = 50;
        [SerializeField] private float cellSize = 1;
        [SerializeField] private Vector3 originPosition = new Vector3(0, 0);
        [HideInInspector] public Dictionary<Vector3Int, WorldTile> worldTileDictionary;
        private Dictionary<TileBase, GlobalTileData> globalTileDataDictionary;
        private ZGrid<WorldTile> worldTileGrid;
        private const float TREE_CLUSTER_CHANCE = 0.05f; // in 1/100 percent
        private const float TREE_NEIGHBOR_CHANCE = 5f; // 35 default
        [SerializeField] private TreeData oakTreeData;
        public bool saveMapToFile;
        public bool loadMapFromFile;
        public string fileName = "mapData";
        private string filePath;
        private string spriteAtlasAddress = "Assets/ZetaGamesRPG/Development/TeamSharedAssets/Sprites_Minifantasy/Biome - Forgotten Plains/Tileset/Minifactory_Grassland.spriteatlas";
       
        private void Start() {
            // Initialize
            filePath = @"d:\GameDevProjects\Team\ZetaRPG\Assets\ZetaGamesRPG\Development\Zach\Json\" + fileName + ".json";
            worldTileGrid = new ZGrid<WorldTile>(mapWidth, mapHeight, cellSize, originPosition, (ZGrid<WorldTile> g, int x, int y) => new WorldTile(g, x, y));
            globalTileDataDictionary = new Dictionary<TileBase, GlobalTileData>();
            worldTileDictionary = new Dictionary<Vector3Int, WorldTile>();

            if (loadMapFromFile) {
                CreateMapFromJson();
            } else {
                // Create dictionary of all BaseTiles and their global data
                CreateGlobalDataDict();

                // Create master world tile dictionary. Will hold individual data on every tile in the game.
                CreateWorldTileDict();
            }

            if (saveMapToFile) {
                SaveJson();
            } else {
                // Randomize world features using world tile dictionary
             //   CreateRandomTrees();

                // Update Astar pathfinding graph to include world tile info
             //   UpdatePathfindingGrid();
            }
        }

        public ZGrid<WorldTile> GetGrid() {
            return worldTileGrid;
        }

        public WorldTile GetWorldTile(Vector3Int gridPos) {
            if (worldTileDictionary.TryGetValue(gridPos, out WorldTile tile)) {
                return tile;
            } else {
                return null;
            }
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

        private void SaveJson() {
            string jsonSaveString = JsonConvert.SerializeObject(worldTileDictionary.Values, Formatting.None);
            File.WriteAllText(filePath, jsonSaveString);
        }

        private void LoadJson() {
            string jsonLoadData = File.ReadAllText(filePath);
            List<WorldTile> worldTiles = JsonConvert.DeserializeObject<List<WorldTile>>(jsonLoadData);

            foreach (WorldTile tile in worldTiles) {
                Vector3Int gridPos = new Vector3Int(tile.x, tile.y, 0);
                worldTileDictionary.Add(gridPos, tile);
            }
        }

        private void CreateMapFromJson() {
            // Populate world tile dictionary from json file
            LoadJson();

            Debug.Log("Json loaded");

            // Populate tilemap asynchronously with info from each world tile
            foreach (WorldTile worldTile in worldTileDictionary.Values) {
                Debug.Log("Iterating through world tiles in dictionary");
                foreach (Tilemap tilemap in mapList) {
                    Debug.Log("Iterating through tile maps in mapList");
                    if (tilemap.name.Equals(worldTile.tilemap)) {
                        Debug.Log("Tilemap name matched, loading sprite async next");
                        //   Tile tile = ScriptableObject.CreateInstance<Tile>();
                        //   string atlasedSpriteAddress = spriteAtlasAddress + '[' + worldTile.spriteName + ']';
                        //   var asyncOperationHandle = Addressables.LoadAssetAsync<Sprite>(atlasedSpriteAddress);
                        //   tile.sprite = asyncOperationHandle.Result;
                        //   tilemap.SetTile(new Vector3Int(worldTile.x, worldTile.y, 0), tile);
                        StartCoroutine(LoadSpriteAsync(worldTile, tilemap));
                    } else {
                        Debug.Log("Did not find a matching tilemap for this world tile");
                    }
                }
            }
        }

        private IEnumerator LoadSpriteAsync(WorldTile worldTile, Tilemap tilemap) {
            Debug.Log("LoadSpriteAsync called");
            string atlasedSpriteAddress = spriteAtlasAddress + '[' + worldTile.spriteName + ']';
            var asyncOperationHandle = Addressables.LoadAssetAsync<Sprite>(atlasedSpriteAddress);
            

            Debug.Log("Async status: " + asyncOperationHandle.Status.ToString());

            yield return asyncOperationHandle;
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = asyncOperationHandle.Result;

            if (asyncOperationHandle.Status.Equals(AsyncOperationStatus.Succeeded)) {
                Debug.Log("Tile sprite loaded successfully");
            } else {
                Debug.Log("Tile sprite load failed");
            }

            Vector3 worldPos = worldTileGrid.GetWorldPosition(worldTile.x, worldTile.y);
            Vector3Int tilemapPos = new Vector3Int((int)worldPos.x, (int)worldPos.y, 0);
            tilemap.SetTile(tilemapPos, tile);
            
            // Release at some point
            yield return new WaitForSeconds(6);
            Addressables.Release(asyncOperationHandle);
        }

        private void UpdatePathfindingGrid() {
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
                        node.Penalty = (uint)(1000 * worldTile.pathPenalty);

                        // terrain type sets node tag

                    } else {
                        Debug.Log("Node does not align with world tile");
                    }
                });
            });
        }

        private void CreateGlobalDataDict() {
            foreach (GlobalTileData tileData in globalTileDataList) {
                foreach (TileBase tile in tileData.tiles) {
                    if (!globalTileDataDictionary.ContainsKey(tile)) {
                        globalTileDataDictionary.Add(tile, tileData);
                    }
                }
            }
        }

        private void CreateWorldTileDict() {
            for (int gridX = 0; gridX < worldTileGrid.GetWidth(); gridX++) {
                for (int gridY = 0; gridY < worldTileGrid.GetHeight(); gridY++) {
                    Vector3Int gridPos = new Vector3Int(gridX, gridY, 0);
                    Vector3 worldPos = worldTileGrid.GetWorldPosition(gridPos.x, gridPos.y);
                    Vector3Int tilemapPos = new Vector3Int((int)worldPos.x, (int)worldPos.y, 0);

                    foreach (Tilemap tilemap in mapList) {
                        WorldTile worldTile = worldTileGrid.GetGridObject(gridPos);
                        TileBase tile = tilemap.GetTile(tilemapPos);

                        if (tile != null) {
                            if (globalTileDataDictionary.TryGetValue(tile, out GlobalTileData globalTileData)) {
                                // set the global data per tile
                                worldTile.pathPenalty = globalTileData.pathPenalty;
                                worldTile.walkable = globalTileData.walkable;
                                worldTile.type = globalTileData.type;
                                worldTile.speedPercent = globalTileData.speedPercent;

                                // set tilemap data
                                worldTile.spriteName = tilemap.GetSprite(tilemapPos).name;
                                worldTile.tilemap = tilemap.name;

                                // add world tile to dictionary (each world tile is unique in this dictionary)
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
        }

        private void CreateRandomTrees() {
            for (int gridX = 0; gridX < worldTileGrid.GetWidth(); gridX++) {
                for (int gridY = 0; gridY < worldTileGrid.GetHeight(); gridY++) {
                    Vector3Int gridPos = new Vector3Int(gridX, gridY, 0);

                    if (worldTileDictionary.TryGetValue(gridPos, out WorldTile currentTile)) {
                        // if current tile is grass, then plant trees!
                        if (currentTile.type == "grass" && !currentTile.occupied) {
                            // Cache neighbor tiles (one tile away)
                            // NW - N - NE
                            // W  - X - E
                            // SW - S - SE
                            WorldTile nw;
                            WorldTile n;
                            WorldTile ne;
                            WorldTile w;
                            WorldTile e;
                            WorldTile sw;
                            WorldTile s;
                            WorldTile se;

                            Vector3Int nwPos = new Vector3Int(gridX - 1, gridY + 1, 0);
                            Vector3Int nPos = new Vector3Int(gridX, gridY + 1, 0);
                            Vector3Int nePos = new Vector3Int(gridX + 1, gridY + 1, 0);
                            Vector3Int wPos = new Vector3Int(gridX - 1, gridY, 0);
                            Vector3Int ePos = new Vector3Int(gridX + 1, gridY, 0);
                            Vector3Int swPos = new Vector3Int(gridX - 1, gridY - 1, 0);
                            Vector3Int sPos = new Vector3Int(gridX, gridY - 1, 0);
                            Vector3Int sePos = new Vector3Int(gridX + 1, gridY - 1, 0);

                            if (worldTileDictionary.ContainsKey(nwPos)) {
                                nw = worldTileDictionary[nwPos];
                            } else continue;
                            if (worldTileDictionary.ContainsKey(nPos)) {
                                n = worldTileDictionary[nPos];
                            } else continue;
                            if (worldTileDictionary.ContainsKey(nePos)) {
                                ne = worldTileDictionary[nePos];
                            } else continue;
                            if (worldTileDictionary.ContainsKey(wPos)) {
                                w = worldTileDictionary[wPos];
                            } else continue;
                            if (worldTileDictionary.ContainsKey(ePos)) {
                                e = worldTileDictionary[ePos];
                            } else continue;
                            if (worldTileDictionary.ContainsKey(swPos)) {
                                sw = worldTileDictionary[swPos];
                            } else continue;
                            if (worldTileDictionary.ContainsKey(sPos)) {
                                s = worldTileDictionary[sPos];
                            } else continue;
                            if (worldTileDictionary.ContainsKey(sePos)) {
                                se = worldTileDictionary[sePos];
                            } else continue;

                            // Cache neighbor tiles (two tiles away)
                            WorldTile nw2;
                            WorldTile n2;
                            WorldTile ne2;
                            WorldTile w2;
                            WorldTile e2;
                            WorldTile sw2;
                            WorldTile s2;
                            WorldTile se2;

                            Vector3Int nw2Pos = new Vector3Int(gridX - 2, gridY + 2, 0);
                            Vector3Int n2Pos = new Vector3Int(gridX, gridY + 2, 0);
                            Vector3Int ne2Pos = new Vector3Int(gridX + 2, gridY + 2, 0);
                            Vector3Int w2Pos = new Vector3Int(gridX - 2, gridY, 0);
                            Vector3Int e2Pos = new Vector3Int(gridX + 2, gridY, 0);
                            Vector3Int sw2Pos = new Vector3Int(gridX - 2, gridY - 2, 0);
                            Vector3Int s2Pos = new Vector3Int(gridX, gridY - 2, 0);
                            Vector3Int se2Pos = new Vector3Int(gridX + 2, gridY - 2, 0);

                            if (worldTileDictionary.ContainsKey(nw2Pos)) {
                                nw2 = worldTileDictionary[nw2Pos];
                            } else continue;
                            if (worldTileDictionary.ContainsKey(n2Pos)) {
                                n2 = worldTileDictionary[n2Pos];
                            } else continue;
                            if (worldTileDictionary.ContainsKey(ne2Pos)) {
                                ne2 = worldTileDictionary[ne2Pos];
                            } else continue;
                            if (worldTileDictionary.ContainsKey(w2Pos)) {
                                w2 = worldTileDictionary[w2Pos];
                            } else continue;
                            if (worldTileDictionary.ContainsKey(e2Pos)) {
                                e2 = worldTileDictionary[e2Pos];
                            } else continue;
                            if (worldTileDictionary.ContainsKey(sw2Pos)) {
                                sw2 = worldTileDictionary[sw2Pos];
                            } else continue;
                            if (worldTileDictionary.ContainsKey(s2Pos)) {
                                s2 = worldTileDictionary[s2Pos];
                            } else continue;
                            if (worldTileDictionary.ContainsKey(se2Pos)) {
                                se2 = worldTileDictionary[se2Pos];
                            } else continue;

                            Vector3 currentTileWorldPos = worldTileGrid.GetWorldPosition(gridX, gridY);

                            // If those neighbors are not occupied, then allow random chance of spawning first tree in a cluster
                            if (!nw.occupied && !n.occupied && !ne.occupied && !w.occupied && !e.occupied && !sw.occupied && !s.occupied && !se.occupied
                                && !nw2.occupied && !n2.occupied && !ne2.occupied && !w2.occupied && !e2.occupied && !sw2.occupied && !s2.occupied && !se2.occupied) {
                                if (Random.Range(0, 100f) <= TREE_CLUSTER_CHANCE) {
                                    Instantiate(oakTreeData.prefab, currentTileWorldPos, Quaternion.identity);
                                    currentTile.occupied = true;
                                    currentTile.walkable = false;
                                    currentTile.occupiedType = "tree";

                                    // set additional tiles unwalkable depending on size of tree
                                    foreach (Vector3Int modPos in oakTreeData.adjacentGridOccupation) {
                                        WorldTile otherTile = worldTileDictionary[gridPos + modPos];
                                        otherTile.occupied = true;
                                        otherTile.walkable = false;
                                        otherTile.occupiedType = "tree";
                                    }

                                    Debug.Log("Tree cluster seeded");
                                }
                            } else if (!nw.occupied && !n.occupied && !ne.occupied && !w.occupied && !e.occupied && !sw.occupied && !s.occupied && !se.occupied) {
                                if (nw2.occupiedType.Equals("tree") || n2.occupiedType.Equals("tree") || ne2.occupiedType.Equals("tree") || w2.occupiedType.Equals("tree")
                                 || e2.occupiedType.Equals("tree") || sw2.occupiedType.Equals("tree") || s2.occupiedType.Equals("tree") || se2.occupiedType.Equals("tree")) {
                                    // Spawn a tree with good chances if there is a neighbor tree
                                    if (Random.Range(0, 100f) <= TREE_NEIGHBOR_CHANCE) {
                                        Instantiate(oakTreeData.prefab, currentTileWorldPos, Quaternion.identity);
                                        currentTile.occupied = true;
                                        currentTile.walkable = false;
                                        currentTile.occupiedType = "tree";

                                        // set additional tiles unwalkable depending on size of tree
                                        foreach (Vector3Int modPos in oakTreeData.adjacentGridOccupation) {
                                            WorldTile otherTile = worldTileDictionary[gridPos + modPos];
                                            otherTile.occupied = true;
                                            otherTile.walkable = false;
                                            otherTile.occupiedType = "tree";
                                        }

                                        Debug.Log("Tree neighbor created");
                                    }
                                }
                            }
                        }
                    } else {
                        Debug.LogWarning("Tree Randomizer: WorldTile not found with gridPos " + gridPos);
                    }
                }
            }
        }
    }
}
