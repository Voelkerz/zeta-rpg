using Pathfinding;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    public class CommunityManager : MonoBehaviour {

        [HideInInspector] public static CommunityManager Instance;
        [HideInInspector] public List<Settlement> settlementList;
        [HideInInspector] public Dictionary<Vector3, WorldTile[]> viableRegions;

        public int smallSettlementSize { get => 10; }

        private List<string> humanSettlementNames = new List<string> {
            "Faywater",
            "Blackburn",
            "Pryham",
            "Westloch",
            "Eriwall",
            "Whitehill",
            "Newhurst",
            "Belland",
            "Woodriver",
            "Linhaven",
            "Aldmoor",
            "Orland",
            "Esterford",
            "Aelston",
            "Ostford",
            "Mistwick"
            };

        private void Awake() {
            Instance = this;
            settlementList = new List<Settlement>();
            viableRegions = new Dictionary<Vector3, WorldTile[]>();
        }

        private void Start() {
            DetermineViableSettlementRegions();
        }

        public Settlement GetRandomSettlement() {
            List<Settlement> viableSettlements = new List<Settlement>();

            foreach (Settlement settlement in settlementList) {
                if (!settlement.atMax) {
                    viableSettlements.Add(settlement);
                }
            }

            if (viableSettlements.Count > 0) {
                return viableSettlements[Random.Range(0, viableSettlements.Count)];
            } else {
                return null;
            }
        }

        public Settlement JoinCommunity(string settlementName, GameObject citizen) {
            foreach (Settlement settlement in settlementList) {
                if (settlement.settlementName.Equals(settlementName)) {
                    settlement.citizenList.Add(citizen);
                    return settlement;
                }
            }

            return null;
        }

        public void LeaveSettlement(Settlement settlement, GameObject citizen) {
            settlement.citizenList.Remove(citizen);
            Debug.Log("Leaving settlement: " + settlement.settlementName + " || Population: " + settlement.citizenList.Count);
        }

        public Settlement CreateNewSettlement(GameObject leader) {
            // Choose random name for settlement
            string settlementName = humanSettlementNames[Random.Range(0, humanSettlementNames.Count)];
            humanSettlementNames.Remove(settlementName);

            // Create settlement with leader and name
            Settlement settlement = new Settlement(settlementName, leader);
            settlement.maxPopulation = smallSettlementSize;

            // Pick a random region out of the list of viable regions to settle
            Vector3[] keyArray = new Vector3[viableRegions.Count];
            viableRegions.Keys.CopyTo(keyArray, 0);
            Vector3 randKey = keyArray[Random.Range(0, keyArray.Length)];
            settlement.originRegion = new Settlement.Region((int)randKey.x, (int)randKey.y);
            settlement.growthRegions.Add(settlement.originRegion);

            // Remove chosen region from the viable list
            viableRegions.Remove(randKey);

            // Add neighboring regions in a 3x3 grid around settlement to the occupied settlement list for future growth
            for (int x = 0; x < 3; x++) {
                for (int y = 0; y < 3; y++) {
                    Vector3 regionKey = new Vector3(randKey.x + (x - 1), randKey.y + (y - 1), 0);
                    if (viableRegions.ContainsKey(regionKey)) {
                        settlement.growthRegions.Add(new Settlement.Region((int)regionKey.x, (int)regionKey.y));
                    }
                }
            }

            // Remove neighboring regions in a 9x9 grid around the settlement from viable settlement list to create spacing.
            for (int x = 0; x < 9; x++) {
                for (int y = 0; y < 9; y++) {
                    Vector3 regionKey = new Vector3(randKey.x + (x - 4), randKey.y + (y - 4), 0);
                    if (viableRegions.ContainsKey(regionKey)) {
                        viableRegions.Remove(regionKey);
                    }
                }
            }

            // Set region as settled (will set neighbor regions also)
            settlement.SetSettledRegion(settlement.originRegion.x, settlement.originRegion.y);

            // Create community bulletin board
            WorldTile[] originRegionTiles = MapManager.Instance.GetWorldRegionGrid().GetGridObject(settlement.originRegion.x, settlement.originRegion.y);
            WorldTile bulletinBoardTile;

            while (true) {
                bulletinBoardTile = originRegionTiles[Random.Range(0, originRegionTiles.Length)];
                if (bulletinBoardTile.walkable && !bulletinBoardTile.occupied) break;
            }

            // Adjust bulletin board tile
            settlement.bulletinBoardPos = bulletinBoardTile.GetWorldPosition();
            bulletinBoardTile.occupied = true;
            bulletinBoardTile.occupiedStatus = ZetaUtilities.OCCUPIED_STRUCTURE;

            // TODO: make barrier based on race
            // Create initial barrier around settlement
            List<WorldTile> allUpdatedTiles = CreateInitialSettlementBarrier(settlement);

            // Create main road(s) through settlement center
            //allUpdatedTiles.AddRange(CalculateInnerRoads(settlement.originRegion));

            // Calculate road connections to other settlements
            //CalculateOuterRoads(settlement);

            // Update Astar graph for tile walkability
            ZetaUtilities.UpdateMultipleAstarGraphNodes(allUpdatedTiles);

            // Add to total settlement list
            settlementList.Add(settlement);

            return settlement;
        }

        private void DetermineViableSettlementRegions() {
            // Determine viable regions to build settlements
            ZetaGrid<WorldTile[]> regionGrid = MapManager.Instance.GetWorldRegionGrid();
            Vector3Int regionPos = new Vector3Int(0, 0, 0);
            int mapWidth = MapManager.Instance.mapWidth;
            int mapHeight = MapManager.Instance.mapHeight;
            int regionSize = MapManager.Instance.regionSize;

            for (int regionX = 0; regionX < mapWidth / regionSize; regionX++) {
                for (int regionY = 0; regionY < mapHeight / regionSize; regionY++) {
                    float numGround = 0;
                    bool hasNullNeighbor = false;
                    int adjGridSize = 3;
                    int step = 1;

                    for (int x = 0; x < adjGridSize; x++) {
                        for (int y = 0; y < adjGridSize; y++) {
                            if (Mathf.Abs(x - step) < step && Mathf.Abs(y - step) < step) {
                                // skip inner tiles
                                continue;
                            }
                            // check grid bounds
                            if (!regionGrid.IsWithinGridBounds(regionX + (x - step), regionY + (y - step))) {
                                hasNullNeighbor = true;
                            }
                        }
                    }

                    // Null neighbor means a border region
                    if (hasNullNeighbor) {
                        continue;
                    }

                    // Get the region
                    WorldTile[] regionTileList = regionGrid.GetGridObject(regionX, regionY);

                    // Add up the ground tiles within the region
                    foreach (WorldTile tile in regionTileList) {
                        if (tile.terrainType.Equals(ZetaUtilities.TERRAIN_GRASS)) {
                            numGround++;
                        }
                    }

                    // If number of ground tiles is over 80% then add to viable region list
                    if ((numGround / regionTileList.Length) >= 1f) {
                        regionPos.x = regionX;
                        regionPos.y = regionY;
                        viableRegions.Add(regionPos, regionTileList);
                    }
                }
            }

            Debug.Log("Viable Regions: " + viableRegions.Count);
        }

        private List<WorldTile> CreateInitialSettlementBarrier(Settlement settlement) {
            WorldTile[] originRegionTiles = MapManager.Instance.GetWorldRegionGrid().GetGridObject(settlement.originRegion.x, settlement.originRegion.y);
            List<WorldTile> updatedTiles;

            WorldTile swCornerTile;
            WorldTile seCornerTile;
            WorldTile neCornerTile;
            WorldTile nwCornerTile;
            List<WorldTile> westWallTiles = new List<WorldTile>();
            List<WorldTile> eastWallTiles = new List<WorldTile>();
            List<WorldTile> northWallTiles = new List<WorldTile>();
            List<WorldTile> southWallTiles = new List<WorldTile>();

            MapObstacle barrierSE = null;
            MapObstacle barrierSW = null;
            MapObstacle barrierNE = null;
            MapObstacle barrierNW = null;
            List<MapObstacle> barriersE = new List<MapObstacle>();
            List<MapObstacle> barriersW = new List<MapObstacle>();
            List<MapObstacle> barriersS = new List<MapObstacle>();
            List<MapObstacle> barriersN = new List<MapObstacle>();

            bool createNorthEntrance = true;
            bool createSouthEntrance = true;
            bool createWestEntrance = true;
            bool createEastEntrance = true;

            foreach (MapObstacle obstacle in TilemapObstacleManager.Instance.tilemapObstacles) {
                string spriteName = obstacle.spriteName;
                switch (spriteName) {
                    case "Minifantasy_AdventurersCampsite_WoodBarrier_SW":
                        barrierSW = obstacle;
                        break;
                    case "Minifantasy_AdventurersCampsite_WoodBarrier_SE":
                        barrierSE = obstacle;
                        break;
                    case "Minifantasy_AdventurersCampsite_WoodBarrier_NE":
                        barrierNE = obstacle;
                        break;
                    case "Minifantasy_AdventurersCampsite_WoodBarrier_NW":
                        barrierNW = obstacle;
                        break;
                    case "Minifantasy_AdventurersCampsite_WoodBarrier_E1":
                    case "Minifantasy_AdventurersCampsite_WoodBarrier_E2":
                        barriersE.Add(obstacle);
                        break;
                    case "Minifantasy_AdventurersCampsite_WoodBarrier_W1":
                    case "Minifantasy_AdventurersCampsite_WoodBarrier_W2":
                        barriersW.Add(obstacle);
                        break;
                    case "Minifantasy_AdventurersCampsite_WoodBarrier_S1":
                    case "Minifantasy_AdventurersCampsite_WoodBarrier_S2":
                        barriersS.Add(obstacle);
                        break;
                    case "Minifantasy_AdventurersCampsite_WoodBarrier_N1":
                    case "Minifantasy_AdventurersCampsite_WoodBarrier_N2":
                        barriersN.Add(obstacle);
                        break;
                    default:
                        break;
                }
            }

            // SW Corner
            swCornerTile = originRegionTiles[0];
            settlement.boundaryWallTiles.Add(swCornerTile);
            updatedTiles = swCornerTile.SetParentTileObstacle(barrierSW, ZetaUtilities.TILEMAP_OBSTACLE + swCornerTile.elevation, ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_CORNER, ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_ADJACENT);

            // SE Corner
            seCornerTile = MapManager.Instance.GetWorldTileGrid().GetGridObject(swCornerTile.x + MapManager.Instance.regionSize - 1, swCornerTile.y);
            settlement.boundaryWallTiles.Add(seCornerTile);
            updatedTiles.AddRange(seCornerTile.SetParentTileObstacle(barrierSE, ZetaUtilities.TILEMAP_OBSTACLE + seCornerTile.elevation, ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_CORNER, ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_ADJACENT));

            // NE Corner
            neCornerTile = MapManager.Instance.GetWorldTileGrid().GetGridObject(seCornerTile.x, seCornerTile.y + MapManager.Instance.regionSize - 1);
            settlement.boundaryWallTiles.Add(neCornerTile);
            updatedTiles.AddRange(neCornerTile.SetParentTileObstacle(barrierNE, ZetaUtilities.TILEMAP_OBSTACLE + neCornerTile.elevation, ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_CORNER, ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_ADJACENT));

            // NW Corner
            nwCornerTile = MapManager.Instance.GetWorldTileGrid().GetGridObject(neCornerTile.x - MapManager.Instance.regionSize + 1, neCornerTile.y);
            settlement.boundaryWallTiles.Add(nwCornerTile);
            updatedTiles.AddRange(nwCornerTile.SetParentTileObstacle(barrierNW, ZetaUtilities.TILEMAP_OBSTACLE + nwCornerTile.elevation, ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_CORNER, ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_ADJACENT));

            // Check neighboring regions for viable entrance. No entrance if the neighbor region is not viable to expand settlement.
            // South region
            WorldTile[] neighborRegion = MapManager.Instance.GetWorldRegionGrid().GetGridObject(settlement.originRegion.x, settlement.originRegion.y - 1);
            foreach (WorldTile southRegionTile in neighborRegion) {
                if (southRegionTile.terrainType.Equals(ZetaUtilities.TERRAIN_WATER)) {
                    createSouthEntrance = false;
                    break;
                }
            }

            // North region
            neighborRegion = MapManager.Instance.GetWorldRegionGrid().GetGridObject(settlement.originRegion.x, settlement.originRegion.y + 1);
            foreach (WorldTile northRegionTile in neighborRegion) {
                if (northRegionTile.terrainType.Equals(ZetaUtilities.TERRAIN_WATER)) {
                    createNorthEntrance = false;
                    break;
                }
            }

            // West region
            neighborRegion = MapManager.Instance.GetWorldRegionGrid().GetGridObject(settlement.originRegion.x - 1, settlement.originRegion.y);
            foreach (WorldTile westRegionTile in neighborRegion) {
                if (westRegionTile.terrainType.Equals(ZetaUtilities.TERRAIN_WATER)) {
                    createWestEntrance = false;
                    break;
                }
            }

            // East region
            neighborRegion = MapManager.Instance.GetWorldRegionGrid().GetGridObject(settlement.originRegion.x + 1, settlement.originRegion.y);
            foreach (WorldTile eastRegionTile in neighborRegion) {
                if (eastRegionTile.terrainType.Equals(ZetaUtilities.TERRAIN_WATER)) {
                    createEastEntrance = false;
                    break;
                }
            }

            // West side barrier
            for (int i = 0; i < MapManager.Instance.regionSize; i++) {
                WorldTile tile = MapManager.Instance.GetWorldTileGrid().GetGridObject(swCornerTile.x, swCornerTile.y + i);
                westWallTiles.Add(tile);
                settlement.boundaryWallTiles.Add(tile);

                // Chance to make an entrance to settlement
                if (createWestEntrance && (Random.Range(0f, 100f) < 5 || i == MapManager.Instance.regionSize - 7)) {
                    if (i > 7 && i < MapManager.Instance.regionSize - 6) {
                        // Check if occupied already
                        if (tile.tileObstacle != null) {
                            updatedTiles.AddRange(tile.RemoveTileObstacle(ZetaUtilities.TILEMAP_OBSTACLE));
                        }

                        // make an entrance
                        tile.occupied = true;
                        tile.hasParent = false;
                        tile.occupiedStatus = ZetaUtilities.OCCUPIED_SETTLEMENT_ENTRANCE;
                        tile.walkable = true;

                        MapManager.Instance.tileMapList[ZetaUtilities.TILEMAP_OBSTACLE + tile.elevation].SetTile(tile.GetWorldPositionInt(), null);
                        MapManager.Instance.tileMapList[ZetaUtilities.TILEMAP_OBSTACLE_SHADOW + tile.elevation].SetTile(tile.GetWorldPositionInt(), null);

                        // make next tile an entrance as well
                        WorldTile nextTile = MapManager.Instance.GetWorldTileGrid().GetGridObject(tile.x, tile.y + 1);

                        // Check if occupied already
                        if (nextTile.tileObstacle != null) {
                            updatedTiles.AddRange(nextTile.RemoveTileObstacle(ZetaUtilities.TILEMAP_OBSTACLE + nextTile.elevation));
                        }

                        nextTile.occupied = true;
                        nextTile.hasParent = false;
                        nextTile.occupiedStatus = ZetaUtilities.OCCUPIED_SETTLEMENT_ENTRANCE;
                        nextTile.walkable = true;

                        MapManager.Instance.tileMapList[ZetaUtilities.TILEMAP_OBSTACLE + nextTile.elevation].SetTile(nextTile.GetWorldPositionInt(), null);
                        MapManager.Instance.tileMapList[ZetaUtilities.TILEMAP_OBSTACLE_SHADOW + nextTile.elevation].SetTile(nextTile.GetWorldPositionInt(), null);

                        // Adjust settlement region and neighbor region exit data
                        settlement.originRegion.westExit = true;
                        settlement.originRegion.westExitPos.x = tile.x;
                        settlement.originRegion.westExitPos.y = tile.y;

                        if (settlement.ContainsRegion(settlement.originRegion.x - 1, settlement.originRegion.y)) {
                            Settlement.Region neighbor = settlement.GetSettledRegion(settlement.originRegion.x - 1, settlement.originRegion.y);
                            neighbor.eastExit = true;
                            neighbor.eastExitPos.x = tile.x - 1;
                            neighbor.eastExitPos.y = tile.y;
                        }

                        createWestEntrance = false;
                        continue;
                    }
                }

                // Load a barrier
                if (!tile.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_CORNER) && !tile.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_WEST) && !tile.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_ADJACENT) && !tile.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_SETTLEMENT_ENTRANCE)) {
                    int randNum = Random.Range(0, barriersW.Count);
                    updatedTiles.AddRange(tile.SetParentTileObstacle(barriersW[randNum], ZetaUtilities.TILEMAP_OBSTACLE + tile.elevation, ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_WEST, ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_ADJACENT));
                }
            }

            // East side barrier
            for (int i = 0; i < MapManager.Instance.regionSize; i++) {
                WorldTile tile = MapManager.Instance.GetWorldTileGrid().GetGridObject(seCornerTile.x, seCornerTile.y + i);
                eastWallTiles.Add(tile);
                settlement.boundaryWallTiles.Add(tile);

                // Chance to make an entrance to settlement
                if (createEastEntrance && (Random.Range(0f, 100f) < 5 || i == MapManager.Instance.regionSize - 7)) {
                    if (i > 7 && i < MapManager.Instance.regionSize - 6) {
                        // Check if occupied already
                        if (tile.tileObstacle != null) {
                            updatedTiles.AddRange(tile.RemoveTileObstacle(ZetaUtilities.TILEMAP_OBSTACLE + tile.elevation));
                        }

                        // make an entrance
                        tile.occupied = true;
                        tile.hasParent = false;
                        tile.occupiedStatus = ZetaUtilities.OCCUPIED_SETTLEMENT_ENTRANCE;
                        tile.walkable = true;

                        MapManager.Instance.tileMapList[ZetaUtilities.TILEMAP_OBSTACLE + tile.elevation].SetTile(tile.GetWorldPositionInt(), null);
                        MapManager.Instance.tileMapList[ZetaUtilities.TILEMAP_OBSTACLE_SHADOW + tile.elevation].SetTile(tile.GetWorldPositionInt(), null);

                        // make next tile an entrance as well
                        WorldTile nextTile = MapManager.Instance.GetWorldTileGrid().GetGridObject(tile.x, tile.y + 1);

                        // Check if occupied already
                        if (nextTile.tileObstacle != null) {
                            updatedTiles.AddRange(nextTile.RemoveTileObstacle(ZetaUtilities.TILEMAP_OBSTACLE + nextTile.elevation));
                        }

                        nextTile.occupied = true;
                        nextTile.hasParent = false;
                        nextTile.occupiedStatus = ZetaUtilities.OCCUPIED_SETTLEMENT_ENTRANCE;
                        nextTile.walkable = true;

                        MapManager.Instance.tileMapList[ZetaUtilities.TILEMAP_OBSTACLE + nextTile.elevation].SetTile(nextTile.GetWorldPositionInt(), null);
                        MapManager.Instance.tileMapList[ZetaUtilities.TILEMAP_OBSTACLE_SHADOW + nextTile.elevation].SetTile(nextTile.GetWorldPositionInt(), null);

                        // Adjust settlement region and neighbor region exit data
                        settlement.originRegion.eastExit = true;
                        settlement.originRegion.eastExitPos.x = tile.x;
                        settlement.originRegion.eastExitPos.y = tile.y;

                        if (settlement.ContainsRegion(settlement.originRegion.x + 1, settlement.originRegion.y)) {
                            Settlement.Region neighbor = settlement.GetSettledRegion(settlement.originRegion.x + 1, settlement.originRegion.y);
                            neighbor.westExit = true;
                            neighbor.westExitPos.x = tile.x + 1;
                            neighbor.westExitPos.y = tile.y;
                        }

                        createEastEntrance = false;
                        continue;
                    }
                }

                // Load a barrier
                if (!tile.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_CORNER) && !tile.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_EAST) && !tile.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_ADJACENT) && !tile.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_SETTLEMENT_ENTRANCE)) {
                    int randNum = Random.Range(0, barriersE.Count);
                    updatedTiles.AddRange(tile.SetParentTileObstacle(barriersE[randNum], ZetaUtilities.TILEMAP_OBSTACLE + tile.elevation, ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_EAST, ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_ADJACENT));
                }
            }

            // South side barrier
            for (int i = 0; i < MapManager.Instance.regionSize; i++) {
                WorldTile tile = MapManager.Instance.GetWorldTileGrid().GetGridObject(swCornerTile.x + i, swCornerTile.y);
                southWallTiles.Add(tile);
                settlement.boundaryWallTiles.Add(tile);

                // Chance to make an entrance to settlement
                if (createSouthEntrance && (Random.Range(0f, 100f) < 5 || i == MapManager.Instance.regionSize - 7)) {
                    if (i > 7 && i < MapManager.Instance.regionSize - 6) {
                        // Check if occupied already
                        if (tile.tileObstacle != null) {
                            updatedTiles.AddRange(tile.RemoveTileObstacle(ZetaUtilities.TILEMAP_OBSTACLE + tile.elevation));
                        }

                        // make an entrance
                        tile.occupied = true;
                        tile.hasParent = false;
                        tile.occupiedStatus = ZetaUtilities.OCCUPIED_SETTLEMENT_ENTRANCE;
                        tile.walkable = true;

                        MapManager.Instance.tileMapList[ZetaUtilities.TILEMAP_OBSTACLE + tile.elevation].SetTile(tile.GetWorldPositionInt(), null);
                        MapManager.Instance.tileMapList[ZetaUtilities.TILEMAP_OBSTACLE_SHADOW + tile.elevation].SetTile(tile.GetWorldPositionInt(), null);

                        // make next tile an entrance as well
                        WorldTile nextTile = MapManager.Instance.GetWorldTileGrid().GetGridObject(tile.x + 1, tile.y);

                        // Check if occupied already
                        if (nextTile.tileObstacle != null) {
                            updatedTiles.AddRange(nextTile.RemoveTileObstacle(ZetaUtilities.TILEMAP_OBSTACLE + nextTile.elevation));
                        }

                        nextTile.occupied = true;
                        tile.hasParent = false;
                        nextTile.occupiedStatus = ZetaUtilities.OCCUPIED_SETTLEMENT_ENTRANCE;
                        nextTile.walkable = true;

                        MapManager.Instance.tileMapList[ZetaUtilities.TILEMAP_OBSTACLE + nextTile.elevation].SetTile(nextTile.GetWorldPositionInt(), null);
                        MapManager.Instance.tileMapList[ZetaUtilities.TILEMAP_OBSTACLE_SHADOW + nextTile.elevation].SetTile(nextTile.GetWorldPositionInt(), null);

                        // Adjust settlement region and neighbor region exit data
                        settlement.originRegion.southExit = true;
                        settlement.originRegion.southExitPos.x = tile.x;
                        settlement.originRegion.southExitPos.y = tile.y;

                        if (settlement.ContainsRegion(settlement.originRegion.x, settlement.originRegion.y - 1)) {
                            Settlement.Region neighbor = settlement.GetSettledRegion(settlement.originRegion.x, settlement.originRegion.y - 1);
                            neighbor.northExit = true;
                            neighbor.northExitPos.x = tile.x;
                            neighbor.northExitPos.y = tile.y - 1;
                        }

                        createSouthEntrance = false;
                        continue;
                    }
                }

                // Load a barrier
                if (!tile.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_CORNER) && !tile.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_SOUTH) && !tile.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_SETTLEMENT_ENTRANCE)) {
                    int randNum = Random.Range(0, barriersS.Count);
                    updatedTiles.AddRange(tile.SetParentTileObstacle(barriersS[randNum], ZetaUtilities.TILEMAP_OBSTACLE + tile.elevation, ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_SOUTH, ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_ADJACENT));
                }
            }

            // North side barrier
            for (int i = 0; i < MapManager.Instance.regionSize - 1; i++) {
                WorldTile tile = MapManager.Instance.GetWorldTileGrid().GetGridObject(nwCornerTile.x + 1 + i, nwCornerTile.y);
                northWallTiles.Add(tile);
                settlement.boundaryWallTiles.Add(tile);

                // Chance to make an entrance to settlement
                if (createNorthEntrance && (Random.Range(0f, 100f) < 5 || i == MapManager.Instance.regionSize - 7)) {
                    if (i > 7 && i < MapManager.Instance.regionSize - 6) {
                        // Check if occupied already
                        if (tile.tileObstacle != null) {
                            updatedTiles.AddRange(tile.RemoveTileObstacle(ZetaUtilities.TILEMAP_OBSTACLE + tile.elevation));
                        }

                        // make an entrance
                        tile.occupied = true;
                        tile.hasParent = false;
                        tile.occupiedStatus = ZetaUtilities.OCCUPIED_SETTLEMENT_ENTRANCE;
                        tile.walkable = true;

                        MapManager.Instance.tileMapList[ZetaUtilities.TILEMAP_OBSTACLE + tile.elevation].SetTile(tile.GetWorldPositionInt(), null);
                        MapManager.Instance.tileMapList[ZetaUtilities.TILEMAP_OBSTACLE_SHADOW + tile.elevation].SetTile(tile.GetWorldPositionInt(), null);

                        // make next tile an entrance as well
                        WorldTile nextTile = MapManager.Instance.GetWorldTileGrid().GetGridObject(tile.x + 1, tile.y);

                        // Check if occupied already
                        if (nextTile.tileObstacle != null) {
                            updatedTiles.AddRange(nextTile.RemoveTileObstacle(ZetaUtilities.TILEMAP_OBSTACLE + nextTile.elevation));
                        }

                        nextTile.occupied = true;
                        tile.hasParent = false;
                        nextTile.occupiedStatus = ZetaUtilities.OCCUPIED_SETTLEMENT_ENTRANCE;
                        nextTile.walkable = true;

                        MapManager.Instance.tileMapList[ZetaUtilities.TILEMAP_OBSTACLE + nextTile.elevation].SetTile(nextTile.GetWorldPositionInt(), null);
                        MapManager.Instance.tileMapList[ZetaUtilities.TILEMAP_OBSTACLE_SHADOW + nextTile.elevation].SetTile(nextTile.GetWorldPositionInt(), null);

                        // Adjust settlement region and neighbor region exit data
                        settlement.originRegion.northExit = true;
                        settlement.originRegion.northExitPos.x = tile.x;
                        settlement.originRegion.northExitPos.y = tile.y;

                        if (settlement.ContainsRegion(settlement.originRegion.x, settlement.originRegion.y + 1)) {
                            Settlement.Region neighbor = settlement.GetSettledRegion(settlement.originRegion.x, settlement.originRegion.y + 1);
                            neighbor.southExit = true;
                            neighbor.southExitPos.x = tile.x;
                            neighbor.southExitPos.y = tile.y + 1;
                        }

                        createNorthEntrance = false;
                        continue;
                    }
                }

                // Load a barrier
                if (!tile.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_CORNER) && !tile.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_NORTH) && !tile.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_SETTLEMENT_ENTRANCE)) {
                    int randNum = Random.Range(0, barriersN.Count);
                    updatedTiles.AddRange(tile.SetParentTileObstacle(barriersN[randNum], ZetaUtilities.TILEMAP_OBSTACLE + tile.elevation, ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_NORTH, ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_ADJACENT));
                }
            }

            // return the updated tile list
            return updatedTiles;
        }
    }
}
