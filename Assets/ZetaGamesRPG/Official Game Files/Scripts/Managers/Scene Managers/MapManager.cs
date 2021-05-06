using System.Collections.Generic;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using UnityEngine.Tilemaps;
using Newtonsoft.Json;


namespace ZetaGames.RPG {
    public class MapManager : MonoBehaviour {

        public static MapManager Instance;
        public List<Tilemap> tileMapList;
        [SerializeField] private List<GlobalTileData> globalTileDataList;
        [SerializeField] private List<BaseObject> mapFeaturesDataList;
        [SerializeField] private int mapWidth = 256;
        [SerializeField] private int mapHeight = 256;
        private ZetaGrid<WorldTile> worldTileGrid;
        public float treeStarterChance = 0.1f;
        public float treeAdjacencyChance = 15f;
        public float stoneStarterChance = 0.05f;
        public float stoneAdjacencyChance = 5f;
        public float flowerStarterChance = 0.5f;
        public float flowerAdjacencyChance = 50;
        public bool saveMapToFile;
        public bool loadMapFromFile;
        public string fileName = "mapData";
        private string filePath;
        private string grasslandSpriteAtlas = "Assets/ZetaGamesRPG/Official Game Files/SpriteAtlas/Grassland.spriteatlas";

        private void Awake() {
            // Create instance
            Instance = this;

            // Initialize
            filePath = @"d:\GameDevProjects\Team\ZetaRPG\Assets\ZetaGamesRPG\Development\Zach\Json\" + fileName + ".json";
            worldTileGrid = new ZetaGrid<WorldTile>(mapWidth, mapHeight, 1, new Vector3(0, 0), (int x, int y) => new WorldTile(x, y));

            if (loadMapFromFile) {
                //LoadJson();
            } else {
                // Fill world tile grid with data. Will hold individual data on every tile in the game.
                FillWorldTileGrid();
            }

            if (saveMapToFile) {
                //SaveJson();
            } else {
                // Randomize world features
                CreateRandomTrees();
                CreateRandomStoneNodes();

                // Load flowers last. They will generate around "occupied tiles"
                CreateRandomFlowers();
            }
        }

        private void Start() {
            // Update Astar pathfinding graph to include world tile info ((MUST BE IN START))
            UpdatePathfindingGrid();
        }

        public ZetaGrid<WorldTile> GetWorldTileGrid() {
            return worldTileGrid;
        }


        private void Update() {

            if (Input.GetMouseButtonDown(0)) {
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                worldTileGrid.GetXY(mouseWorldPos, out int x, out int y);
                Vector3Int mouseGridPos = new Vector3Int(x, y, 0);

                WorldTile clickedTile = worldTileGrid.GetGridObject(mouseWorldPos);

                //Debug.Log("Occupied: " + clickedTile.occupied + " || Occupied Status: " + clickedTile.occupiedStatus + " || Occupied Type: " + clickedTile.occupiedType + " || GameObject: " + clickedTile.HasTileObject());

                //mapList[2].SetTile(mouseGridPos, null);
            }
        }

        /*
        private void SaveJson() {
            List<WorldTile> mapTileList = new List<WorldTile>();
            Vector3Int mapGridPos = new Vector3Int();

            for (int mapX = 0; mapX < worldTileGrid.GetWidth(); mapX++) {
                for (int mapY = 0; mapY < worldTileGrid.GetHeight(); mapY++) {
                    mapGridPos.x = mapX;
                    mapGridPos.y = mapY;
                    mapGridPos.z = 0;
                    mapTileList.Add(worldTileGrid.GetGridObject(mapGridPos));
                }
            }

            string jsonSaveString = JsonConvert.SerializeObject(mapTileList, Formatting.None);
            File.WriteAllText(filePath, jsonSaveString);
        }

        private void LoadJson() {
            string jsonLoadData = File.ReadAllText(filePath);
            List<WorldTile> mapTileList = JsonConvert.DeserializeObject<List<WorldTile>>(jsonLoadData);

            foreach (WorldTile tile in mapTileList) {
                // world tile grid holds only the tile info for the highest priority tile (higher tilemap index)
                worldTileGrid.SetGridObject(tile.x, tile.y, tile);
                // chunk grid will hold all tiles so that it can render everything
                chunkGrid.GetGridObject(tile.chunkX, tile.chunkY).Add(tile);
            }
        }
        */

        /*
        private void CreateMapFromJson() {
            // Populate world tile dictionary from json file
            LoadJson();
            Debug.Log("Json loaded");

            // Populate tilemap asynchronously with info from each world tile
            foreach (var tileList in mapChunkDictionary.Values) {
                foreach (WorldTile worldTile in tileList.Values) {
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
        }
        */

        public IEnumerator SetAtlasedSpriteAsync(WorldTile worldTile, int tilemapIndex, string spriteName) {
            string atlasedSpriteAddress = grasslandSpriteAtlas + '[' + spriteName + ']';
            var asyncOperationHandle = Addressables.LoadAssetAsync<Sprite>(atlasedSpriteAddress);

            if (worldTile.tileSprites.ContainsKey(tilemapIndex)) {
                worldTile.tileSprites[tilemapIndex] = spriteName;
            } else {
                worldTile.tileSprites.Add(tilemapIndex, spriteName);
            }

            yield return asyncOperationHandle;
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = asyncOperationHandle.Result;

            Vector3 worldPos = worldTileGrid.GetWorldPosition(worldTile.x, worldTile.y);
            Vector3Int tilemapPos = new Vector3Int((int)worldPos.x, (int)worldPos.y, 0);
            tileMapList[tilemapIndex].SetTile(tilemapPos, tile);

            // Release at some point
            yield return new WaitForSeconds(3);
            Addressables.Release(asyncOperationHandle);
        }

        public IEnumerator PlayResourceSpriteAnimation(WorldTile worldTile, int tilemapIndex, List<string> spriteNames, int startIndex) {
            int curIndex = startIndex;

            // first animation frame
            string atlasedSpriteAddress = grasslandSpriteAtlas + '[' + spriteNames[curIndex] + ']';
            var asyncOperationHandle = Addressables.LoadAssetAsync<Sprite>(atlasedSpriteAddress);
            yield return asyncOperationHandle;
            Tile frame1 = ScriptableObject.CreateInstance<Tile>();
            frame1.sprite = asyncOperationHandle.Result;

            // first animation frame shadow
            atlasedSpriteAddress = grasslandSpriteAtlas + '[' + spriteNames[curIndex + 1] + ']';
            asyncOperationHandle = Addressables.LoadAssetAsync<Sprite>(atlasedSpriteAddress);
            yield return asyncOperationHandle;
            Tile frame1_shadow = ScriptableObject.CreateInstance<Tile>();
            frame1_shadow.sprite = asyncOperationHandle.Result;

            // second animation frame
            atlasedSpriteAddress = grasslandSpriteAtlas + '[' + spriteNames[curIndex + 2] + ']';
            asyncOperationHandle = Addressables.LoadAssetAsync<Sprite>(atlasedSpriteAddress);
            yield return asyncOperationHandle;
            Tile frame2 = ScriptableObject.CreateInstance<Tile>();
            frame2.sprite = asyncOperationHandle.Result;

            // second animation frame shadow
            atlasedSpriteAddress = grasslandSpriteAtlas + '[' + spriteNames[curIndex + 3] + ']';
            asyncOperationHandle = Addressables.LoadAssetAsync<Sprite>(atlasedSpriteAddress);
            yield return asyncOperationHandle;
            Tile frame2_shadow = ScriptableObject.CreateInstance<Tile>();
            frame2_shadow.sprite = asyncOperationHandle.Result;

            // third animation frame
            atlasedSpriteAddress = grasslandSpriteAtlas + '[' + spriteNames[curIndex + 4] + ']';
            asyncOperationHandle = Addressables.LoadAssetAsync<Sprite>(atlasedSpriteAddress);
            yield return asyncOperationHandle;
            Tile frame3 = ScriptableObject.CreateInstance<Tile>();
            frame3.sprite = asyncOperationHandle.Result;

            // third animation frame shadow
            atlasedSpriteAddress = grasslandSpriteAtlas + '[' + spriteNames[curIndex + 5] + ']';
            asyncOperationHandle = Addressables.LoadAssetAsync<Sprite>(atlasedSpriteAddress);
            yield return asyncOperationHandle;
            Tile frame3_shadow = ScriptableObject.CreateInstance<Tile>();
            frame3_shadow.sprite = asyncOperationHandle.Result;

            Vector3 worldPos = worldTileGrid.GetWorldPosition(worldTile.x, worldTile.y);
            Vector3Int tilemapPos = new Vector3Int((int)worldPos.x, (int)worldPos.y, 0);

            yield return new WaitForEndOfFrame();
            tileMapList[tilemapIndex].SetTile(tilemapPos, frame1);
            tileMapList[tilemapIndex + 1].SetTile(tilemapPos, frame1_shadow);
            yield return new WaitForSeconds(0.33333333f);
            tileMapList[tilemapIndex].SetTile(tilemapPos, frame2);
            tileMapList[tilemapIndex + 1].SetTile(tilemapPos, frame2_shadow);
            yield return new WaitForSeconds(0.33333333f);
            tileMapList[tilemapIndex].SetTile(tilemapPos, frame3);
            tileMapList[tilemapIndex + 1].SetTile(tilemapPos, frame3_shadow);
            yield return new WaitForSeconds(0.33333333f);

            // Release at some point
            yield return new WaitForSeconds(1);
            Addressables.Release(asyncOperationHandle);
        }

        private void UpdatePathfindingGrid() {
            AstarPath.active.AddWorkItem(() => {
                var gg = AstarPath.active.data.gridGraph;
                Vector3Int mapTilePos = new Vector3Int();

                gg.GetNodes(node => {
                    // Find world tile based on current node position
                    worldTileGrid.GetXY((Vector3)node.position, out int mapX, out int mapY);

                    if (worldTileGrid.IsWithinGridBounds(mapX, mapY)) {
                        mapTilePos.x = mapX;
                        mapTilePos.y = mapY;
                        mapTilePos.z = 0;

                        WorldTile worldTile = worldTileGrid.GetGridObject(mapTilePos);

                        node.Penalty = (uint)(1000 * worldTile.pathPenalty);

                        if (!worldTile.walkable) {
                            node.Walkable = false;
                        }
                    }
                });
            });
        }

        private void FillWorldTileGrid() {
            // Create global dictionary for world creation use
            Dictionary<TileBase, GlobalTileData> globalTileDataDictionary = new Dictionary<TileBase, GlobalTileData>();

            foreach (GlobalTileData tileData in globalTileDataList) {
                foreach (TileBase tile in tileData.tiles) {
                    if (!globalTileDataDictionary.ContainsKey(tile)) {
                        globalTileDataDictionary.Add(tile, tileData);
                    }
                }
            }

            // Iterate through every map grid square
            for (int mapX = 0; mapX < worldTileGrid.GetWidth(); mapX++) {
                for (int mapY = 0; mapY < worldTileGrid.GetHeight(); mapY++) {
                    WorldTile worldTile = worldTileGrid.GetGridObject(mapX, mapY);
                    Vector3Int mapGridPos = new Vector3Int(mapX, mapY, 0);

                    foreach (Tilemap tilemap in tileMapList) {
                        TileBase tile = tilemap.GetTile(mapGridPos);

                        if (tile != null) {
                            if (globalTileDataDictionary.TryGetValue(tile, out GlobalTileData globalTileData)) {
                                // set the global data per tile
                                worldTile.pathPenalty = globalTileData.pathPenalty;
                                worldTile.walkable = globalTileData.walkable;
                                worldTile.terrainType = globalTileData.type;
                                worldTile.speedPercent = globalTileData.speedPercent;

                                // set tilemap data
                                worldTile.tileSprites.Add(tileMapList.IndexOf(tilemap), tilemap.GetSprite(mapGridPos).name);
                            } else {
                                Debug.LogWarning("TileBase not found in global tile dictionary: " + tile.name);
                            }
                        }
                    }
                }
            }
        }

        private void CreateRandomTrees() {
            // Cache tree data
            ResourceNodeData oakTreeData = null;

            foreach (var feature in mapFeaturesDataList) {
                if (typeof(ResourceNodeData).IsInstanceOfType(feature)) {
                    ResourceNodeData temp = (ResourceNodeData)feature;
                    if (temp.resourceType.Equals(ResourceType.Oak)) {
                        oakTreeData = temp;
                    }
                }
            }

            for (int mapX = 0; mapX < worldTileGrid.GetWidth(); mapX++) {
                for (int mapY = 0; mapY < worldTileGrid.GetHeight(); mapY++) {
                    List<WorldTile> combinedNeighbors = new List<WorldTile>();
                    List<WorldTile> oneTileNeighbors = new List<WorldTile>();
                    List<WorldTile> twoTileNeighbors = new List<WorldTile>();
                    List<WorldTile> threeTileNeighbors = new List<WorldTile>();

                    int step;
                    int adjGridSize;
                    bool neighborsOccupied = false;

                    // get current tile
                    WorldTile currentTile = worldTileGrid.GetGridObject(mapX, mapY);

                    // if current tile is grass and unoccupied, then plant trees!
                    if (currentTile.terrainType == ZetaUtilities.TERRAIN_GRASS && !currentTile.occupied) {
                        // NW - N - NE
                        // W  - X - E
                        // SW - S - SE

                        // Cache neighbor tiles one tile away (3x3 grid with one step from origin)
                        adjGridSize = 3;
                        step = 1;

                        for (int x = 0; x < adjGridSize; x++) {
                            for (int y = 0; y < adjGridSize; y++) {
                                if (Mathf.Abs(x - step) < step && Mathf.Abs(y - step) < step) {
                                    // skip inner tiles
                                    continue;
                                }
                                // check grid bounds
                                if (worldTileGrid.IsWithinGridBounds(mapX + (x - step), mapY + (y - step))) {
                                    WorldTile neighborTile = worldTileGrid.GetGridObject(mapX + (x - step), mapY + (y - step));
                                    oneTileNeighbors.Add(neighborTile);
                                }
                            }
                        }

                        // Cache neighbor tiles two tiles away (5x5 grid with two steps from origin)
                        adjGridSize = 5;
                        step = 2;

                        for (int x = 0; x < adjGridSize; x++) {
                            for (int y = 0; y < adjGridSize; y++) {
                                if (Mathf.Abs(x - step) < step && Mathf.Abs(y - step) < step) {
                                    // skip inner tiles
                                    continue;
                                }
                                // check grid bounds
                                if (worldTileGrid.IsWithinGridBounds(mapX + (x - step), mapY + (y - step))) {
                                    WorldTile neighborTile = worldTileGrid.GetGridObject(mapX + (x - step), mapY + (y - step));
                                    twoTileNeighbors.Add(neighborTile);
                                }
                            }
                        }

                        // Cache neighbor tiles three tiles away (7x7 grid with three steps from origin)
                        adjGridSize = 7;
                        step = 3;

                        for (int x = 0; x < adjGridSize; x++) {
                            for (int y = 0; y < adjGridSize; y++) {
                                if (Mathf.Abs(x - step) < step && Mathf.Abs(y - step) < step) {
                                    // skip inner tiles
                                    continue;
                                }
                                // check grid bounds
                                if (worldTileGrid.IsWithinGridBounds(mapX + (x - step), mapY + (y - step))) {
                                    WorldTile neighborTile = worldTileGrid.GetGridObject(mapX + (x - step), mapY + (y - step));
                                    threeTileNeighbors.Add(neighborTile);
                                }
                            }
                        }

                        // If all those neighbors are not occupied by a tree, then allow random chance of spawning first tree in a cluster
                        combinedNeighbors.AddRange(oneTileNeighbors);
                        combinedNeighbors.AddRange(twoTileNeighbors);
                        combinedNeighbors.AddRange(threeTileNeighbors);
                        foreach (WorldTile tile in combinedNeighbors) {
                            if (tile.occupied) {
                                if (tile.occupiedStatus.Contains(ZetaUtilities.OCCUPIED_NODE) && tile.occupiedCategory == ResourceCategory.Wood.ToString()) {
                                    neighborsOccupied = true;
                                    break;
                                }
                            }
                        }

                        if (!neighborsOccupied) {
                            if (Random.Range(0, 100f) <= treeStarterChance) {
                                if (currentTile.occupiedStatus != ZetaUtilities.OCCUPIED_NODE_FULL) {
                                    currentTile.occupied = true;
                                    currentTile.walkable = false;
                                    currentTile.occupiedStatus = ZetaUtilities.OCCUPIED_NODE_FULL;
                                    currentTile.occupiedCategory = ResourceCategory.Wood.ToString();

                                    // Specific to oak tree. Will change later
                                    currentTile.occupiedType = ResourceType.Oak.ToString();
                                    currentTile.resourceNodeData = oakTreeData;

                                    StartCoroutine(SetAtlasedSpriteAsync(currentTile, 2, oakTreeData.spriteFull));
                                    StartCoroutine(SetAtlasedSpriteAsync(currentTile, 3, oakTreeData.spriteFullShadow));

                                    // TODO: Possibly do a switch case to determine which tree is planted...base it on a biome type??

                                    // set additional tiles unwalkable depending on size of tree
                                    foreach (var featureData in mapFeaturesDataList) {
                                        if (typeof(ResourceNodeData).IsInstanceOfType(featureData)) {
                                            ResourceNodeData treeData = (ResourceNodeData)featureData;
                                            if (treeData.resourceType.Equals(ResourceType.Oak)) {
                                                foreach (Vector3Int modPos in treeData.adjacentGridOccupation) {
                                                    if (worldTileGrid.IsWithinGridBounds(mapX + modPos.x, mapY + modPos.y)) {
                                                        WorldTile otherTile = worldTileGrid.GetGridObject(mapX + modPos.x, mapY + modPos.y);
                                                        otherTile.occupied = true;
                                                        otherTile.walkable = false;
                                                        otherTile.occupiedStatus = ZetaUtilities.OCCUPIED_NODE_ADJACENT;
                                                        otherTile.occupiedCategory = ResourceCategory.Wood.ToString();

                                                        // Specific to oak tree. Will change later
                                                        otherTile.occupiedType = ResourceType.Oak.ToString();
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    Debug.Log("Tree cluster seeded");
                                }
                            }
                        }

                        // If one and two tile neighbors are not occupied, but a tree is found in a three tile neighbor, then allow a good chance to spawn an additional tree (helps create clusters)
                        neighborsOccupied = false;
                        combinedNeighbors.Clear();
                        combinedNeighbors.AddRange(oneTileNeighbors);
                        combinedNeighbors.AddRange(twoTileNeighbors);

                        foreach (WorldTile tile in combinedNeighbors) {
                            if (tile.occupied) {
                                if (tile.occupiedStatus.Contains(ZetaUtilities.OCCUPIED_NODE) && tile.occupiedCategory == ResourceCategory.Wood.ToString()) {
                                    neighborsOccupied = true;
                                    break;
                                }
                            }
                        }

                        if (!neighborsOccupied) {
                            // check for a tree occupying 3 tile neighbors
                            foreach (WorldTile tile in threeTileNeighbors) {
                                if (tile.occupied) {
                                    if (tile.occupiedStatus.Contains(ZetaUtilities.OCCUPIED_NODE) && tile.occupiedCategory == ResourceCategory.Wood.ToString()) {
                                        neighborsOccupied = true;
                                        break;
                                    }
                                }
                            }

                            // if tree occupies a 3 tile neighbor, then roll to plant a tree
                            if (neighborsOccupied) {
                                if (Random.Range(0, 100f) <= treeAdjacencyChance) {
                                    if (currentTile.occupiedStatus != ZetaUtilities.OCCUPIED_NODE_FULL) {
                                        currentTile.occupied = true;
                                        currentTile.walkable = false;
                                        currentTile.occupiedStatus = ZetaUtilities.OCCUPIED_NODE_FULL;
                                        currentTile.occupiedCategory = ResourceCategory.Wood.ToString();

                                        // Specific to oak tree. Will change later
                                        currentTile.occupiedType = ResourceType.Oak.ToString();
                                        currentTile.resourceNodeData = oakTreeData;

                                        StartCoroutine(SetAtlasedSpriteAsync(currentTile, 2, oakTreeData.spriteFull));
                                        StartCoroutine(SetAtlasedSpriteAsync(currentTile, 3, oakTreeData.spriteFullShadow));

                                        // set additional tiles unwalkable depending on size of tree
                                        foreach (var feature in mapFeaturesDataList) {
                                            if (typeof(ResourceNodeData).IsInstanceOfType(feature)) {
                                                ResourceNodeData tree = (ResourceNodeData)feature;
                                                if (tree.resourceType.Equals(ResourceType.Oak)) {
                                                    foreach (Vector3Int modPos in tree.adjacentGridOccupation) {
                                                        WorldTile otherTile = worldTileGrid.GetGridObject(mapX + modPos.x, mapY + modPos.y);
                                                        otherTile.occupied = true;
                                                        otherTile.walkable = false;
                                                        otherTile.occupiedStatus = ZetaUtilities.OCCUPIED_NODE_ADJACENT;
                                                        otherTile.occupiedCategory = ResourceCategory.Wood.ToString();

                                                        // Specific to oak tree. Will change later
                                                        otherTile.occupiedType = ResourceType.Oak.ToString();
                                                    }
                                                }
                                            }
                                        }

                                        Debug.Log("Tree neighbor created");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void CreateRandomFlowers() {
            // 5 variations of flowers for grass tiles
            List<string> grasslandsFlowerList = new List<string>();
            grasslandsFlowerList.Add("Minifantasy_ForgottenPlainsFlowers_0");
            grasslandsFlowerList.Add("Minifantasy_ForgottenPlainsFlowers_1");
            grasslandsFlowerList.Add("Minifantasy_ForgottenPlainsFlowers_2");
            grasslandsFlowerList.Add("Minifantasy_ForgottenPlainsFlowers_3");
            grasslandsFlowerList.Add("Minifantasy_ForgottenPlainsFlowers_4");
            List<string> grasslandsFlowerShadowList = new List<string>();
            grasslandsFlowerShadowList.Add("Minifantasy_ForgottenPlainsFlowerShadows_0");
            grasslandsFlowerShadowList.Add("Minifantasy_ForgottenPlainsFlowerShadows_1");
            grasslandsFlowerShadowList.Add("Minifantasy_ForgottenPlainsFlowerShadows_2");
            grasslandsFlowerShadowList.Add("Minifantasy_ForgottenPlainsFlowerShadows_3");
            grasslandsFlowerShadowList.Add("Minifantasy_ForgottenPlainsFlowerShadows_4");

            // Iterate through every WorldTile
            for (int mapX = 0; mapX < worldTileGrid.GetWidth(); mapX++) {
                for (int mapY = 0; mapY < worldTileGrid.GetHeight(); mapY++) {
                    // get current tile
                    WorldTile currentTile = worldTileGrid.GetGridObject(mapX, mapY);

                    // if current tile is grass and unoccupied, then plant a flower!
                    if (currentTile.terrainType == ZetaUtilities.TERRAIN_GRASS && !currentTile.occupied) {
                        // pick random flower from list
                        int randomIndex = Random.Range(0, grasslandsFlowerList.Count);
                        string flower = grasslandsFlowerList[randomIndex];
                        string flowerShadow = grasslandsFlowerShadowList[randomIndex];

                        bool planted = false;

                        // low chance to plant flower in tile
                        if (Random.Range(0, 100f) <= flowerStarterChance) {
                            Debug.Log("Creating flower!");
                            planted = true;
                            StartCoroutine(SetAtlasedSpriteAsync(currentTile, 2, flower));
                            StartCoroutine(SetAtlasedSpriteAsync(currentTile, 3, flowerShadow));
                        }

                        // if not planted, check neighbors
                        if (!planted) {
                            int step;
                            int adjGridSize;
                            bool hasFlower = false;

                            // Check neighbor tiles one tile away (3x3 grid with one step from origin)
                            adjGridSize = 3;
                            step = 1;

                            for (int x = 0; x < adjGridSize; x++) {
                                for (int y = 0; y < adjGridSize; y++) {
                                    if (Mathf.Abs(x - step) < step && Mathf.Abs(y - step) < step) {
                                        // skip inner tiles
                                        continue;
                                    }
                                    // check grid bounds
                                    if (worldTileGrid.IsWithinGridBounds(mapX + (x - step), mapY + (y - step))) {
                                        WorldTile neighborTile = worldTileGrid.GetGridObject(mapX + (x - step), mapY + (y - step));

                                        // if a neighbor tile has same flower type
                                        foreach (string spriteName in neighborTile.tileSprites.Values) {
                                            if (spriteName.Contains(flower)) {
                                                hasFlower = true;
                                                break;
                                            }
                                        }

                                        if (hasFlower) {
                                            break;
                                        }
                                    }
                                }

                                if (hasFlower) {
                                    break;
                                }
                            }

                            if (!hasFlower) {
                                // Check neighbor tiles two tiles away (5x5 grid with two steps from origin)
                                adjGridSize = 5;
                                step = 2;

                                for (int x = 0; x < adjGridSize; x++) {
                                    for (int y = 0; y < adjGridSize; y++) {
                                        if (Mathf.Abs(x - step) < step && Mathf.Abs(y - step) < step) {
                                            // skip inner tiles
                                            continue;
                                        }
                                        // check grid bounds
                                        if (worldTileGrid.IsWithinGridBounds(mapX + (x - step), mapY + (y - step))) {
                                            WorldTile neighborTile = worldTileGrid.GetGridObject(mapX + (x - step), mapY + (y - step));

                                            // if a neighbor tile has same flower type
                                            foreach (string spriteName in neighborTile.tileSprites.Values) {
                                                if (spriteName.Contains(flower)) {
                                                    hasFlower = true;
                                                    break;
                                                }
                                            }

                                            if (hasFlower) {
                                                break;
                                            }
                                        }
                                    }

                                    if (hasFlower) {
                                        break;
                                    }
                                }
                            }


                            if (hasFlower) {
                                if (Random.Range(0, 100f) <= flowerAdjacencyChance) {
                                    // if a neighbor tile has same flower type, then plant!
                                    Debug.Log("Creating flower neighbor!");
                                    StartCoroutine(SetAtlasedSpriteAsync(currentTile, 2, flower));
                                    StartCoroutine(SetAtlasedSpriteAsync(currentTile, 3, flowerShadow));
                                }
                            }
                        }
                    }
                }
            }
        }

        public void CreateRandomStoneNodes() {
            // Cache stone node data
            List<ResourceNodeData> stoneNodeList = new List<ResourceNodeData>();

            foreach (var feature in mapFeaturesDataList) {
                if (typeof(ResourceNodeData).IsInstanceOfType(feature)) {
                    ResourceNodeData temp = (ResourceNodeData)feature;

                    if (temp.resourceType.Equals(ResourceType.Small) || temp.resourceType.Equals(ResourceType.Large)) {
                        stoneNodeList.Add(temp);
                    }
                }
            }

            // Iterate through every WorldTile
            for (int mapX = 0; mapX < worldTileGrid.GetWidth(); mapX++) {
                for (int mapY = 0; mapY < worldTileGrid.GetHeight(); mapY++) {

                    // get current tile
                    WorldTile currentTile = worldTileGrid.GetGridObject(mapX, mapY);

                    // if current tile is grass and unoccupied, then place a starter stone node!
                    if (currentTile.terrainType == ZetaUtilities.TERRAIN_GRASS && !currentTile.occupied) {
                        // pick random stone node from list
                        int randomIndex = Random.Range(0, stoneNodeList.Count);
                        string stoneNode = stoneNodeList[randomIndex].spriteFull;
                        string stoneShadow = stoneNodeList[randomIndex].spriteFullShadow;

                        bool placed = false;

                        // low chance to place starter node in tile
                        if (Random.Range(0, 100f) <= stoneStarterChance) {
                            Debug.Log("Creating stone node starter!");
                            placed = true;
                            Debug.Log("Placing: " + stoneNodeList[randomIndex].spriteFull);

                            // set additional tiles unwalkable depending on size of node (nullify placement if node won't fit)
                            foreach (Vector3Int modPos in stoneNodeList[randomIndex].adjacentGridOccupation) {
                                if (worldTileGrid.IsWithinGridBounds(mapX + modPos.x, mapY + modPos.y)) {
                                    WorldTile otherTile = worldTileGrid.GetGridObject(mapX + modPos.x, mapY + modPos.y);

                                    if (otherTile.occupied) {
                                        placed = false;
                                        break;
                                    }

                                    otherTile.occupied = true;
                                    otherTile.walkable = false;
                                    otherTile.occupiedStatus = ZetaUtilities.OCCUPIED_NODE_ADJACENT;
                                    otherTile.occupiedCategory = ResourceCategory.Stone.ToString();
                                    otherTile.occupiedType = stoneNodeList[randomIndex].resourceType.ToString();
                                }
                            }

                            if (placed) {
                                currentTile.occupied = true;
                                currentTile.occupiedCategory = ResourceCategory.Stone.ToString();
                                currentTile.occupiedType = stoneNodeList[randomIndex].resourceType.ToString();
                                currentTile.occupiedStatus = ZetaUtilities.OCCUPIED_NODE_FULL;
                                currentTile.walkable = false;

                                StartCoroutine(SetAtlasedSpriteAsync(currentTile, 2, stoneNode));
                                StartCoroutine(SetAtlasedSpriteAsync(currentTile, 3, stoneShadow));
                            } else {
                                break;
                            }
                        }

                        // if not placed, check neighbors
                        if (!placed) {
                            int step;
                            int adjGridSize;

                            // Check neighbor tiles one tile away (3x3 grid with one step from origin)
                            adjGridSize = 5;
                            step = 2;

                            for (int x = 0; x < adjGridSize; x++) {
                                for (int y = 0; y < adjGridSize; y++) {
                                    if (Mathf.Abs(x - step) < step && Mathf.Abs(y - step) < step) {
                                        // skip inner tiles
                                        continue;
                                    }
                                    // check grid bounds
                                    if (worldTileGrid.IsWithinGridBounds(mapX + (x - step), mapY + (y - step))) {
                                        WorldTile neighborTile = worldTileGrid.GetGridObject(mapX + (x - step), mapY + (y - step));

                                        // if a neighbor tile has a stone node
                                        if (neighborTile.occupiedStatus == ZetaUtilities.OCCUPIED_NODE_FULL && neighborTile.occupiedCategory == ResourceCategory.Stone.ToString()) {
                                            placed = true;
                                            break;
                                        }
                                    }
                                }

                                if (placed) {
                                    break;
                                }
                            }


                            if (!placed) {
                                // Check neighbor tiles two tiles away (5x5 grid with two steps from origin)
                                adjGridSize = 7;
                                step = 3;

                                for (int x = 0; x < adjGridSize; x++) {
                                    for (int y = 0; y < adjGridSize; y++) {
                                        if (Mathf.Abs(x - step) < step && Mathf.Abs(y - step) < step) {
                                            // skip inner tiles
                                            continue;
                                        }
                                        // check grid bounds
                                        if (worldTileGrid.IsWithinGridBounds(mapX + (x - step), mapY + (y - step))) {
                                            WorldTile neighborTile = worldTileGrid.GetGridObject(mapX + (x - step), mapY + (y - step));

                                            // if a neighbor tile has a stone node
                                            if (neighborTile.occupiedStatus == ZetaUtilities.OCCUPIED_NODE_FULL && neighborTile.occupiedCategory == ResourceCategory.Stone.ToString()) {
                                                placed = true;
                                                break;
                                            }
                                        }
                                    }

                                    if (placed) {
                                        break;
                                    }
                                }
                            }


                            if (placed) {
                                if (Random.Range(0, 100f) <= stoneAdjacencyChance) {
                                    Debug.Log("Creating stone node neighbor!");

                                    // set additional tiles unwalkable depending on size of node (nullify placement if node won't fit)
                                    foreach (Vector3Int modPos in stoneNodeList[randomIndex].adjacentGridOccupation) {
                                        WorldTile otherTile = worldTileGrid.GetGridObject(mapX + modPos.x, mapY + modPos.y);

                                        if (otherTile.occupied) {
                                            placed = false;
                                            break;
                                        }

                                        otherTile.occupied = true;
                                        otherTile.walkable = false;
                                        otherTile.occupiedStatus = ZetaUtilities.OCCUPIED_NODE_ADJACENT;
                                        otherTile.occupiedCategory = ResourceCategory.Stone.ToString();
                                        otherTile.occupiedType = stoneNodeList[randomIndex].resourceType.ToString();
                                    }

                                    if (placed) {
                                        currentTile.occupied = true;
                                        currentTile.occupiedCategory = ResourceCategory.Stone.ToString();
                                        currentTile.occupiedType = stoneNodeList[randomIndex].resourceType.ToString();
                                        currentTile.occupiedStatus = ZetaUtilities.OCCUPIED_NODE_FULL;
                                        currentTile.walkable = false;

                                        StartCoroutine(SetAtlasedSpriteAsync(currentTile, 2, stoneNode));
                                        StartCoroutine(SetAtlasedSpriteAsync(currentTile, 3, stoneShadow));
                                    } else {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

