using Pathfinding;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    public class CommunityManager : MonoBehaviour {

        [HideInInspector] public static CommunityManager Instance;
        [HideInInspector] public List<Settlement> settlementList;
        //[HideInInspector] public List<Vector3> settlementRegions;
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
            Debug.LogWarning("Leaving settlement: " + settlement.settlementName + " || Population: " + settlement.citizenList.Count);
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

            settlement.bulletinBoardPos = bulletinBoardTile.GetWorldPosition();

            // TODO: make barrier based on race
            // Create initial barrier around settlement
            List<WorldTile> allUpdatedTiles = CreateInitialSettlementBarrier(settlement);

            // Create main road(s) through settlement center
            allUpdatedTiles.AddRange(CalculateRoads(settlement));

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
            updatedTiles = swCornerTile.SetParentTileObstacle(barrierSW, 2, ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_CORNER, ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_ADJACENT);

            // SE Corner
            seCornerTile = MapManager.Instance.GetWorldTileGrid().GetGridObject(swCornerTile.x + MapManager.Instance.regionSize - 1, swCornerTile.y);
            settlement.boundaryWallTiles.Add(seCornerTile);
            updatedTiles.AddRange(seCornerTile.SetParentTileObstacle(barrierSE, 2, ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_CORNER, ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_ADJACENT));

            // NE Corner
            neCornerTile = MapManager.Instance.GetWorldTileGrid().GetGridObject(seCornerTile.x, seCornerTile.y + MapManager.Instance.regionSize - 1);
            settlement.boundaryWallTiles.Add(neCornerTile);
            updatedTiles.AddRange(neCornerTile.SetParentTileObstacle(barrierNE, 2, ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_CORNER, ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_ADJACENT));

            // NW Corner
            nwCornerTile = MapManager.Instance.GetWorldTileGrid().GetGridObject(neCornerTile.x - MapManager.Instance.regionSize + 1, neCornerTile.y);
            settlement.boundaryWallTiles.Add(nwCornerTile);
            updatedTiles.AddRange(nwCornerTile.SetParentTileObstacle(barrierNW, 2, ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_CORNER, ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_ADJACENT));

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
                            updatedTiles.AddRange(tile.RemoveTileObstacle(2));
                        }

                        // make an entrance
                        tile.occupied = true;
                        tile.hasParent = false;
                        tile.occupiedStatus = ZetaUtilities.OCCUPIED_SETTLEMENT_ENTRANCE;
                        tile.walkable = true;

                        MapManager.Instance.tileMapList[2].SetTile(tile.GetWorldPositionInt(), null);
                        MapManager.Instance.tileMapList[3].SetTile(tile.GetWorldPositionInt(), null);

                        // Add number to end of entrance name if more than one on a cardinal side
                        List<Vector3> westEntranceTilePositions = new List<Vector3>();
                        string entName = "West_";
                        int entNum = 0;

                        foreach (string name in settlement.settlementEntrances.Keys) {
                            if (name.Contains(entName)) {
                                entNum++;
                            }
                        }

                        entName += entNum;

                        // Add entrance to vector3 list
                        westEntranceTilePositions.Add(tile.GetWorldPosition());

                        // make next tile an entrance as well
                        WorldTile nextTile = MapManager.Instance.GetWorldTileGrid().GetGridObject(tile.x, tile.y + 1);

                        // Check if occupied already
                        if (nextTile.tileObstacle != null) {
                            updatedTiles.AddRange(nextTile.RemoveTileObstacle(2));
                        }

                        nextTile.occupied = true;
                        tile.hasParent = false;
                        nextTile.occupiedStatus = ZetaUtilities.OCCUPIED_SETTLEMENT_ENTRANCE;
                        nextTile.walkable = true;

                        MapManager.Instance.tileMapList[2].SetTile(nextTile.GetWorldPositionInt(), null);
                        MapManager.Instance.tileMapList[3].SetTile(nextTile.GetWorldPositionInt(), null);

                        // Add entrance to vector3 list
                        westEntranceTilePositions.Add(nextTile.GetWorldPosition());

                        settlement.settlementEntrances.Add(entName, westEntranceTilePositions);

                        // Adjust settlement region and neighbor region exit data
                        settlement.originRegion.westExit = true;

                        if (settlement.ContainsRegion(settlement.originRegion.x - 1, settlement.originRegion.y)) {
                            Settlement.Region neighbor = settlement.GetSettledRegion(settlement.originRegion.x - 1, settlement.originRegion.y);
                            neighbor.eastExit = true;
                        }
                        
                        createWestEntrance = false;
                        continue;
                    }
                }

                // Load a barrier
                if (!tile.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_CORNER) && !tile.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_WEST) && !tile.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_ADJACENT) && !tile.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_SETTLEMENT_ENTRANCE)) {
                    int randNum = Random.Range(0, barriersW.Count);
                    updatedTiles.AddRange(tile.SetParentTileObstacle(barriersW[randNum], 2, ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_WEST, ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_ADJACENT));
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
                            updatedTiles.AddRange(tile.RemoveTileObstacle(2));
                        }

                        // make an entrance
                        tile.occupied = true;
                        tile.hasParent = false;
                        tile.occupiedStatus = ZetaUtilities.OCCUPIED_SETTLEMENT_ENTRANCE;
                        tile.walkable = true;

                        MapManager.Instance.tileMapList[2].SetTile(tile.GetWorldPositionInt(), null);
                        MapManager.Instance.tileMapList[3].SetTile(tile.GetWorldPositionInt(), null);

                        // Add number to end of entrance name if more than one on a cardinal side
                        List<Vector3> eastEntranceTilePositions = new List<Vector3>();
                        string entName = "East_";
                        int entNum = 0;

                        foreach (string name in settlement.settlementEntrances.Keys) {
                            if (name.Contains(entName)) {
                                entNum++;
                            }
                        }

                        entName += entNum;

                        // Add entrance to vector3 list
                        eastEntranceTilePositions.Add(tile.GetWorldPosition());

                        // make next tile an entrance as well
                        WorldTile nextTile = MapManager.Instance.GetWorldTileGrid().GetGridObject(tile.x, tile.y + 1);

                        // Check if occupied already
                        if (nextTile.tileObstacle != null) {
                            updatedTiles.AddRange(nextTile.RemoveTileObstacle(2));
                        }

                        nextTile.occupied = true;
                        tile.hasParent = false;
                        nextTile.occupiedStatus = ZetaUtilities.OCCUPIED_SETTLEMENT_ENTRANCE;
                        nextTile.walkable = true;

                        MapManager.Instance.tileMapList[2].SetTile(nextTile.GetWorldPositionInt(), null);
                        MapManager.Instance.tileMapList[3].SetTile(nextTile.GetWorldPositionInt(), null);

                        // Add entrance to vector3 list
                        eastEntranceTilePositions.Add(nextTile.GetWorldPosition());

                        settlement.settlementEntrances.Add(entName, eastEntranceTilePositions);

                        // Adjust settlement region and neighbor region exit data
                        settlement.originRegion.eastExit = true;

                        if (settlement.ContainsRegion(settlement.originRegion.x + 1, settlement.originRegion.y)) {
                            Settlement.Region neighbor = settlement.GetSettledRegion(settlement.originRegion.x + 1, settlement.originRegion.y);
                            neighbor.westExit = true;
                        }
                        
                        createEastEntrance = false;
                        continue;
                    }
                }

                // Load a barrier
                if (!tile.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_CORNER) && !tile.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_EAST) && !tile.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_ADJACENT) && !tile.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_SETTLEMENT_ENTRANCE)) {
                    int randNum = Random.Range(0, barriersE.Count);
                    updatedTiles.AddRange(tile.SetParentTileObstacle(barriersE[randNum], 2, ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_EAST, ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_ADJACENT));
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
                            updatedTiles.AddRange(tile.RemoveTileObstacle(2));
                        }

                        // make an entrance
                        tile.occupied = true;
                        tile.hasParent = false;
                        tile.occupiedStatus = ZetaUtilities.OCCUPIED_SETTLEMENT_ENTRANCE;
                        tile.walkable = true;

                        MapManager.Instance.tileMapList[2].SetTile(tile.GetWorldPositionInt(), null);
                        MapManager.Instance.tileMapList[3].SetTile(tile.GetWorldPositionInt(), null);

                        // Add number to end of entrance name if more than one on a cardinal side
                        List<Vector3> southEntranceTilePositions = new List<Vector3>();
                        string entName = "South_";
                        int entNum = 0;

                        foreach (string name in settlement.settlementEntrances.Keys) {
                            if (name.Contains(entName)) {
                                entNum++;
                            }
                        }

                        entName += entNum;

                        // Add entrance to vector3 list
                        southEntranceTilePositions.Add(tile.GetWorldPosition());

                        // make next tile an entrance as well
                        WorldTile nextTile = MapManager.Instance.GetWorldTileGrid().GetGridObject(tile.x + 1, tile.y);

                        // Check if occupied already
                        if (nextTile.tileObstacle != null) {
                            updatedTiles.AddRange(nextTile.RemoveTileObstacle(2));
                        }

                        nextTile.occupied = true;
                        tile.hasParent = false;
                        nextTile.occupiedStatus = ZetaUtilities.OCCUPIED_SETTLEMENT_ENTRANCE;
                        nextTile.walkable = true;

                        MapManager.Instance.tileMapList[2].SetTile(nextTile.GetWorldPositionInt(), null);
                        MapManager.Instance.tileMapList[3].SetTile(nextTile.GetWorldPositionInt(), null);

                        // Add entrance to vector3 list
                        southEntranceTilePositions.Add(nextTile.GetWorldPosition());

                        settlement.settlementEntrances.Add(entName, southEntranceTilePositions);

                        // Adjust settlement region and neighbor region exit data
                        settlement.originRegion.southExit = true;

                        if (settlement.ContainsRegion(settlement.originRegion.x, settlement.originRegion.y - 1)) {
                            Settlement.Region neighbor = settlement.GetSettledRegion(settlement.originRegion.x, settlement.originRegion.y - 1);
                            neighbor.northExit = true;
                        }
                        
                        createSouthEntrance = false;
                        continue;
                    }
                }

                // Load a barrier
                if (!tile.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_CORNER) && !tile.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_SOUTH) && !tile.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_SETTLEMENT_ENTRANCE)) {
                    int randNum = Random.Range(0, barriersS.Count);
                    updatedTiles.AddRange(tile.SetParentTileObstacle(barriersS[randNum], 2, ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_SOUTH, ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_ADJACENT));
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
                            updatedTiles.AddRange(tile.RemoveTileObstacle(2));
                        }

                        // make an entrance
                        tile.occupied = true;
                        tile.hasParent = false;
                        tile.occupiedStatus = ZetaUtilities.OCCUPIED_SETTLEMENT_ENTRANCE;
                        tile.walkable = true;

                        MapManager.Instance.tileMapList[2].SetTile(tile.GetWorldPositionInt(), null);
                        MapManager.Instance.tileMapList[3].SetTile(tile.GetWorldPositionInt(), null);

                        // Add number to end of entrance name if more than one on a cardinal side
                        List<Vector3> northEntranceTilePositions = new List<Vector3>();
                        string entName = "North_";
                        int entNum = 0;

                        foreach (string name in settlement.settlementEntrances.Keys) {
                            if (name.Contains(entName)) {
                                entNum++;
                            }
                        }

                        entName += entNum;

                        // Add entrance to vector3 list
                        northEntranceTilePositions.Add(tile.GetWorldPosition());

                        // make next tile an entrance as well
                        WorldTile nextTile = MapManager.Instance.GetWorldTileGrid().GetGridObject(tile.x + 1, tile.y);

                        // Check if occupied already
                        if (nextTile.tileObstacle != null) {
                            updatedTiles.AddRange(nextTile.RemoveTileObstacle(2));
                        }

                        nextTile.occupied = true;
                        tile.hasParent = false;
                        nextTile.occupiedStatus = ZetaUtilities.OCCUPIED_SETTLEMENT_ENTRANCE;
                        nextTile.walkable = true;

                        MapManager.Instance.tileMapList[2].SetTile(nextTile.GetWorldPositionInt(), null);
                        MapManager.Instance.tileMapList[3].SetTile(nextTile.GetWorldPositionInt(), null);

                        // Add entrance to vector3 list
                        northEntranceTilePositions.Add(nextTile.GetWorldPosition());

                        settlement.settlementEntrances.Add(entName, northEntranceTilePositions);

                        // Adjust settlement region and neighbor region exit data
                        settlement.originRegion.northExit = true;
                        
                        if (settlement.ContainsRegion(settlement.originRegion.x, settlement.originRegion.y + 1)) {
                            Settlement.Region neighbor = settlement.GetSettledRegion(settlement.originRegion.x, settlement.originRegion.y + 1);
                            neighbor.southExit = true;
                        }

                        createNorthEntrance = false;
                        continue;
                    }
                }

                // Load a barrier
                if (!tile.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_CORNER) && !tile.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_NORTH) && !tile.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_SETTLEMENT_ENTRANCE)) {
                    int randNum = Random.Range(0, barriersN.Count);
                    updatedTiles.AddRange(tile.SetParentTileObstacle(barriersN[randNum], 2, ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_NORTH, ZetaUtilities.OCCUPIED_SETTLEMENT_WALL_ADJACENT));
                }
            }

            // return the updated tile list
            return updatedTiles;
        }

        private List<WorldTile> CalculateRoads(Settlement settlement) {
            List<WorldTile> updatedTiles = new List<WorldTile>();
            bool north = false;
            bool south = false;
            bool west = false;
            bool east = false;

            foreach (string entrance in settlement.settlementEntrances.Keys) {
                if (entrance.Contains("North")) {
                    north = true;
                } else if (entrance.Contains("South")) {
                    south = true;
                } else if (entrance.Contains("West")) {
                    west = true;
                } else if (entrance.Contains("East")) {
                    east = true;
                }
            }

            // Straight through roads
            if (north && south) updatedTiles.AddRange(CreateThroughRoad_NS(settlement));
            if (east && west) updatedTiles.AddRange(CreateThroughRoad_EW(settlement));

            // Connecting roads (3 entrances)
            if (!east && west && north && south) updatedTiles.AddRange(CreateConnectingRoad_W(settlement));
            if (!west && east && north && south) updatedTiles.AddRange(CreateConnectingRoad_E(settlement));
            if (!south && north && east && west) updatedTiles.AddRange(CreateConnectingRoad_N(settlement));
            if (!north && south && east && west) updatedTiles.AddRange(CreateConnectingRoad_S(settlement));

            return updatedTiles;
        }

        private List<WorldTile> CreateConnectingRoad_S(Settlement settlement) {
            List<WorldTile> updatedTiles = new List<WorldTile>();

            // There must be an AstarPath instance in the scene
            if (AstarPath.active == null) return updatedTiles;

            Vector3 northEnt = settlement.settlementEntrances["South_0"][0] + MapManager.Instance.GetTileOffset();

            for (int i = 0; i < MapManager.Instance.regionSize; i++) {
                WorldTile rightTile = MapManager.Instance.GetWorldTileGrid().GetGridObject((int)northEnt.x + 1, (int)northEnt.y + i);
                WorldTile leftTile = MapManager.Instance.GetWorldTileGrid().GetGridObject((int)northEnt.x, (int)northEnt.y + i);

                if (rightTile.terrainType.Equals(ZetaUtilities.TERRAIN_SETTLEMENT_ROAD) && leftTile.terrainType.Equals(ZetaUtilities.TERRAIN_SETTLEMENT_ROAD)) {
                    break;
                }

                // Remove obstacles blocking path
                if (leftTile.tileObstacle != null) updatedTiles.AddRange(leftTile.RemoveTileObstacle(2));
                if (rightTile.tileObstacle != null) updatedTiles.AddRange(rightTile.RemoveTileObstacle(2));

                // Set path sprites
                MapManager.Instance.SetMapRuleTile(leftTile, 1, "TownDirt_To_Grass");
                MapManager.Instance.SetMapRuleTile(rightTile, 1, "TownDirt_To_Grass");

                // Update tile data
                leftTile.terrainType = ZetaUtilities.TERRAIN_SETTLEMENT_ROAD;
                leftTile.walkable = true;
                rightTile.terrainType = ZetaUtilities.TERRAIN_SETTLEMENT_ROAD;
                rightTile.walkable = true;
            }

            // Fill first two blocks below south entrance

            // left
            WorldTile starterTile = MapManager.Instance.GetWorldTileGrid().GetGridObject((int)northEnt.x, (int)northEnt.y - 1);
            if (starterTile.tileObstacle != null) updatedTiles.AddRange(starterTile.RemoveTileObstacle(2));
            MapManager.Instance.SetMapRuleTile(starterTile, 1, "TownDirt_To_Grass");
            starterTile.terrainType = ZetaUtilities.TERRAIN_SETTLEMENT_ROAD;
            starterTile.walkable = true;

            // right
            starterTile = MapManager.Instance.GetWorldTileGrid().GetGridObject((int)northEnt.x + 1, (int)northEnt.y - 1);
            if (starterTile.tileObstacle != null) updatedTiles.AddRange(starterTile.RemoveTileObstacle(2));
            MapManager.Instance.SetMapRuleTile(starterTile, 1, "TownDirt_To_Grass");
            starterTile.terrainType = ZetaUtilities.TERRAIN_SETTLEMENT_ROAD;
            starterTile.walkable = true;

            return updatedTiles;
        }

        private List<WorldTile> CreateConnectingRoad_N(Settlement settlement) {
            List<WorldTile> updatedTiles = new List<WorldTile>();

            // There must be an AstarPath instance in the scene
            if (AstarPath.active == null) return updatedTiles;

            Vector3 northEnt = settlement.settlementEntrances["North_0"][0] + MapManager.Instance.GetTileOffset();

            for (int i = 0; i < MapManager.Instance.regionSize; i++) {
                WorldTile rightTile = MapManager.Instance.GetWorldTileGrid().GetGridObject((int)northEnt.x + 1, (int)northEnt.y - i);
                WorldTile leftTile = MapManager.Instance.GetWorldTileGrid().GetGridObject((int)northEnt.x, (int)northEnt.y - i);

                if (rightTile.terrainType.Equals(ZetaUtilities.TERRAIN_SETTLEMENT_ROAD) && leftTile.terrainType.Equals(ZetaUtilities.TERRAIN_SETTLEMENT_ROAD)) {
                    break;
                }

                // Remove obstacles blocking path
                if (leftTile.tileObstacle != null) updatedTiles.AddRange(leftTile.RemoveTileObstacle(2));
                if (rightTile.tileObstacle != null) updatedTiles.AddRange(rightTile.RemoveTileObstacle(2));

                // Set path sprites
                MapManager.Instance.SetMapRuleTile(leftTile, 1, "TownDirt_To_Grass");
                MapManager.Instance.SetMapRuleTile(rightTile, 1, "TownDirt_To_Grass");

                // Update tile data
                leftTile.terrainType = ZetaUtilities.TERRAIN_SETTLEMENT_ROAD;
                leftTile.walkable = true;
                rightTile.terrainType = ZetaUtilities.TERRAIN_SETTLEMENT_ROAD;
                rightTile.walkable = true;
            }

            // Fill first two blocks above north entrance

            // left
            WorldTile starterTile = MapManager.Instance.GetWorldTileGrid().GetGridObject((int)northEnt.x, (int)northEnt.y + 1);
            if (starterTile.tileObstacle != null) updatedTiles.AddRange(starterTile.RemoveTileObstacle(2));
            MapManager.Instance.SetMapRuleTile(starterTile, 1, "TownDirt_To_Grass");
            starterTile.terrainType = ZetaUtilities.TERRAIN_SETTLEMENT_ROAD;
            starterTile.walkable = true;

            // right
            starterTile = MapManager.Instance.GetWorldTileGrid().GetGridObject((int)northEnt.x + 1, (int)northEnt.y + 1);
            if (starterTile.tileObstacle != null) updatedTiles.AddRange(starterTile.RemoveTileObstacle(2));
            MapManager.Instance.SetMapRuleTile(starterTile, 1, "TownDirt_To_Grass");
            starterTile.terrainType = ZetaUtilities.TERRAIN_SETTLEMENT_ROAD;
            starterTile.walkable = true;

            return updatedTiles;
        }

        private List<WorldTile> CreateConnectingRoad_E(Settlement settlement) {
            List<WorldTile> updatedTiles = new List<WorldTile>();

            // There must be an AstarPath instance in the scene
            if (AstarPath.active == null) return updatedTiles;

            Vector3 eastEnt = settlement.settlementEntrances["East_0"][0] + MapManager.Instance.GetTileOffset();

            for (int i = 0; i < MapManager.Instance.regionSize; i++) {
                WorldTile upperTile = MapManager.Instance.GetWorldTileGrid().GetGridObject((int)eastEnt.x - i, (int)eastEnt.y + 1);
                WorldTile lowerTile = MapManager.Instance.GetWorldTileGrid().GetGridObject((int)eastEnt.x - i, (int)eastEnt.y);

                if (upperTile.terrainType.Equals(ZetaUtilities.TERRAIN_SETTLEMENT_ROAD) && lowerTile.terrainType.Equals(ZetaUtilities.TERRAIN_SETTLEMENT_ROAD)) {
                    break;
                }

                // Remove obstacles blocking path
                if (lowerTile.tileObstacle != null) updatedTiles.AddRange(lowerTile.RemoveTileObstacle(2));
                if (upperTile.tileObstacle != null) updatedTiles.AddRange(upperTile.RemoveTileObstacle(2));

                // Set path sprites
                MapManager.Instance.SetMapRuleTile(lowerTile, 1, "TownDirt_To_Grass");
                MapManager.Instance.SetMapRuleTile(upperTile, 1, "TownDirt_To_Grass");

                // Update tile data
                lowerTile.terrainType = ZetaUtilities.TERRAIN_SETTLEMENT_ROAD;
                lowerTile.walkable = true;
                upperTile.terrainType = ZetaUtilities.TERRAIN_SETTLEMENT_ROAD;
                upperTile.walkable = true;
            }

            // Fill first two blocks right of east entrance

            // lower
            WorldTile starterTile = MapManager.Instance.GetWorldTileGrid().GetGridObject((int)eastEnt.x + 1, (int)eastEnt.y);
            if (starterTile.tileObstacle != null) updatedTiles.AddRange(starterTile.RemoveTileObstacle(2));
            MapManager.Instance.SetMapRuleTile(starterTile, 1, "TownDirt_To_Grass");
            starterTile.terrainType = ZetaUtilities.TERRAIN_SETTLEMENT_ROAD;
            starterTile.walkable = true;

            // upper
            starterTile = MapManager.Instance.GetWorldTileGrid().GetGridObject((int)eastEnt.x + 1, (int)eastEnt.y + 1);
            if (starterTile.tileObstacle != null) updatedTiles.AddRange(starterTile.RemoveTileObstacle(2));
            MapManager.Instance.SetMapRuleTile(starterTile, 1, "TownDirt_To_Grass");
            starterTile.terrainType = ZetaUtilities.TERRAIN_SETTLEMENT_ROAD;
            starterTile.walkable = true;

            return updatedTiles;
        }

        private List<WorldTile> CreateConnectingRoad_W(Settlement settlement) {
            List<WorldTile> updatedTiles = new List<WorldTile>();

            // There must be an AstarPath instance in the scene
            if (AstarPath.active == null) return updatedTiles;

            Vector3 westEnt = settlement.settlementEntrances["West_0"][0] + MapManager.Instance.GetTileOffset();

            for (int i = 0; i < MapManager.Instance.regionSize; i++) {
                WorldTile upperTile = MapManager.Instance.GetWorldTileGrid().GetGridObject((int)westEnt.x + i, (int)westEnt.y + 1);
                WorldTile lowerTile = MapManager.Instance.GetWorldTileGrid().GetGridObject((int)westEnt.x + i, (int)westEnt.y);

                if (upperTile.terrainType.Equals(ZetaUtilities.TERRAIN_SETTLEMENT_ROAD) && lowerTile.terrainType.Equals(ZetaUtilities.TERRAIN_SETTLEMENT_ROAD)) {
                    break;
                }

                // Remove obstacles blocking path
                if (lowerTile.tileObstacle != null) updatedTiles.AddRange(lowerTile.RemoveTileObstacle(2));
                if (upperTile.tileObstacle != null) updatedTiles.AddRange(upperTile.RemoveTileObstacle(2));

                // Set path sprites
                MapManager.Instance.SetMapRuleTile(lowerTile, 1, "TownDirt_To_Grass");
                MapManager.Instance.SetMapRuleTile(upperTile, 1, "TownDirt_To_Grass");

                // Update tile data
                lowerTile.terrainType = ZetaUtilities.TERRAIN_SETTLEMENT_ROAD;
                lowerTile.walkable = true;
                upperTile.terrainType = ZetaUtilities.TERRAIN_SETTLEMENT_ROAD;
                upperTile.walkable = true;
            }

            // Fill first two blocks left of west entrance

            // lower
            WorldTile starterTile = MapManager.Instance.GetWorldTileGrid().GetGridObject((int)westEnt.x - 1, (int)westEnt.y);
            if (starterTile.tileObstacle != null) updatedTiles.AddRange(starterTile.RemoveTileObstacle(2));
            MapManager.Instance.SetMapRuleTile(starterTile, 1, "TownDirt_To_Grass");
            starterTile.terrainType = ZetaUtilities.TERRAIN_SETTLEMENT_ROAD;
            starterTile.walkable = true;

            // upper
            starterTile = MapManager.Instance.GetWorldTileGrid().GetGridObject((int)westEnt.x - 1, (int)westEnt.y + 1);
            if (starterTile.tileObstacle != null) updatedTiles.AddRange(starterTile.RemoveTileObstacle(2));
            MapManager.Instance.SetMapRuleTile(starterTile, 1, "TownDirt_To_Grass");
            starterTile.terrainType = ZetaUtilities.TERRAIN_SETTLEMENT_ROAD;
            starterTile.walkable = true;

            return updatedTiles;
        }

        private List<WorldTile> CreateThroughRoad_EW(Settlement settlement) {
            List<WorldTile> updatedTiles = new List<WorldTile>();

            // There must be an AstarPath instance in the scene
            if (AstarPath.active == null) return updatedTiles;

            Vector3 eastEnt = settlement.settlementEntrances["East_0"][0] + MapManager.Instance.GetTileOffset();
            Vector3 westEnt = settlement.settlementEntrances["West_0"][0] + MapManager.Instance.GetTileOffset();

            // Fill first two blocks left of west entrance

            // lower
            WorldTile starterTile = MapManager.Instance.GetWorldTileGrid().GetGridObject((int)westEnt.x - 1, (int)westEnt.y);
            if (starterTile.tileObstacle != null) updatedTiles.AddRange(starterTile.RemoveTileObstacle(2));
            MapManager.Instance.SetMapRuleTile(starterTile, 1, "TownDirt_To_Grass");
            starterTile.terrainType = ZetaUtilities.TERRAIN_SETTLEMENT_ROAD;
            starterTile.walkable = true;
            
            // upper
            starterTile = MapManager.Instance.GetWorldTileGrid().GetGridObject((int)westEnt.x - 1, (int)westEnt.y + 1);
            if (starterTile.tileObstacle != null) updatedTiles.AddRange(starterTile.RemoveTileObstacle(2));
            MapManager.Instance.SetMapRuleTile(starterTile, 1, "TownDirt_To_Grass");
            starterTile.terrainType = ZetaUtilities.TERRAIN_SETTLEMENT_ROAD;
            starterTile.walkable = true;

            for (int i = 0; i < 3; i++) {
            var p = ABPath.Construct(westEnt, eastEnt, OnEWPathComplete);
            p.nnConstraint.constrainWalkability = false;
            AstarPath.StartPath(p);
            AstarPath.BlockUntilCalculated(p);
            }

            return updatedTiles;
        }

        private List<WorldTile> CreateThroughRoad_NS(Settlement settlement) {
            List<WorldTile> updatedTiles = new List<WorldTile>();

            // There must be an AstarPath instance in the scene
            if (AstarPath.active == null) return updatedTiles;

            Vector3 northEnt = settlement.settlementEntrances["North_0"][0] + MapManager.Instance.GetTileOffset();
            Vector3 southEnt = settlement.settlementEntrances["South_0"][0] + MapManager.Instance.GetTileOffset();

            // Fill first two blocks below south entrance
            // left
            WorldTile starterTile = MapManager.Instance.GetWorldTileGrid().GetGridObject((int)southEnt.x, (int)southEnt.y - 1);
            if (starterTile.tileObstacle != null) updatedTiles.AddRange(starterTile.RemoveTileObstacle(2));
            MapManager.Instance.SetMapRuleTile(starterTile, 1, "TownDirt_To_Grass");
            starterTile.terrainType = ZetaUtilities.TERRAIN_SETTLEMENT_ROAD;
            starterTile.walkable = true;
            // right
            starterTile = MapManager.Instance.GetWorldTileGrid().GetGridObject((int)southEnt.x + 1, (int)southEnt.y - 1);
            if (starterTile.tileObstacle != null) updatedTiles.AddRange(starterTile.RemoveTileObstacle(2));
            MapManager.Instance.SetMapRuleTile(starterTile, 1, "TownDirt_To_Grass");
            starterTile.terrainType = ZetaUtilities.TERRAIN_SETTLEMENT_ROAD;
            starterTile.walkable = true;

            for (int i = 0; i < 3; i++) {
            var p = ABPath.Construct(southEnt, northEnt, OnNSPathComplete);
            p.nnConstraint.constrainWalkability = false;
            AstarPath.StartPath(p);
            AstarPath.BlockUntilCalculated(p);
            }

            return updatedTiles;
        }

        public void OnEWPathComplete(Path ewPath) {
            if (ewPath.error) {
                Debug.LogError("CommunityManager.OnEWPathComplete(): No valid path found.");
            } else {
                List<WorldTile> updatedTiles = new List<WorldTile>();

                foreach (Vector3 vector in ewPath.vectorPath) {
                    // Get world tiles
                    WorldTile lowerTile = MapManager.Instance.GetWorldTileGrid().GetGridObject((int)vector.x, (int)vector.y);
                    WorldTile upperTile = MapManager.Instance.GetWorldTileGrid().GetGridObject((int)vector.x, (int)vector.y + 1);
                    WorldTile fillerUpperTile = MapManager.Instance.GetWorldTileGrid().GetGridObject((int)vector.x + 1, (int)vector.y + 1);
                    WorldTile fillerLowerTile = MapManager.Instance.GetWorldTileGrid().GetGridObject((int)vector.x + 1, (int)vector.y);

                    // Remove obstacles blocking path
                    if (lowerTile.tileObstacle != null) updatedTiles.AddRange(lowerTile.RemoveTileObstacle(2));
                    if (upperTile.tileObstacle != null) updatedTiles.AddRange(upperTile.RemoveTileObstacle(2));
                    if (fillerUpperTile.tileObstacle != null) updatedTiles.AddRange(fillerUpperTile.RemoveTileObstacle(2));
                    if (fillerLowerTile.tileObstacle != null) updatedTiles.AddRange(fillerLowerTile.RemoveTileObstacle(2));

                    // Set path sprites
                    MapManager.Instance.SetMapRuleTile(lowerTile, 1, "TownDirt_To_Grass");
                    MapManager.Instance.SetMapRuleTile(upperTile, 1, "TownDirt_To_Grass");
                    MapManager.Instance.SetMapRuleTile(fillerUpperTile, 1, "TownDirt_To_Grass");
                    MapManager.Instance.SetMapRuleTile(fillerLowerTile, 1, "TownDirt_To_Grass");

                    // Update tile data
                    lowerTile.terrainType = ZetaUtilities.TERRAIN_SETTLEMENT_ROAD;
                    lowerTile.walkable = true;
                    upperTile.terrainType = ZetaUtilities.TERRAIN_SETTLEMENT_ROAD;
                    upperTile.walkable = true;
                    fillerUpperTile.terrainType = ZetaUtilities.TERRAIN_SETTLEMENT_ROAD;
                    fillerUpperTile.walkable = true;
                    fillerLowerTile.terrainType = ZetaUtilities.TERRAIN_SETTLEMENT_ROAD;
                    fillerLowerTile.walkable = true;
                }

                // Update Astar graph for tile walkability
                ZetaUtilities.UpdateMultipleAstarGraphNodes(updatedTiles);
            }
        }

        public void OnNSPathComplete(Path nsPath) {
            if (nsPath.error) {
                Debug.LogError("CommunityManager.OnNSPathComplete(): No valid path found.");
            } else {
                List<WorldTile> updatedTiles = new List<WorldTile>();

                foreach (Vector3 vector in nsPath.vectorPath) {
                    // Get world tiles
                    WorldTile leftTile = MapManager.Instance.GetWorldTileGrid().GetGridObject((int)vector.x, (int)vector.y);
                    WorldTile rightTile = MapManager.Instance.GetWorldTileGrid().GetGridObject((int)vector.x + 1, (int)vector.y);
                    WorldTile fillerLeftTile = MapManager.Instance.GetWorldTileGrid().GetGridObject((int)vector.x, (int)vector.y + 1);
                    WorldTile fillerRightTile = MapManager.Instance.GetWorldTileGrid().GetGridObject((int)vector.x + 1, (int)vector.y + 1);

                    // Remove obstacles blocking path
                    if (leftTile.tileObstacle != null) updatedTiles.AddRange(leftTile.RemoveTileObstacle(2));
                    if (rightTile.tileObstacle != null) updatedTiles.AddRange(rightTile.RemoveTileObstacle(2));
                    if (fillerLeftTile.tileObstacle != null) updatedTiles.AddRange(fillerLeftTile.RemoveTileObstacle(2));
                    if (fillerRightTile.tileObstacle != null) updatedTiles.AddRange(fillerRightTile.RemoveTileObstacle(2));

                    // Set path sprites
                    MapManager.Instance.SetMapRuleTile(leftTile, 1, "TownDirt_To_Grass");
                    MapManager.Instance.SetMapRuleTile(rightTile, 1, "TownDirt_To_Grass");
                    MapManager.Instance.SetMapRuleTile(fillerLeftTile, 1, "TownDirt_To_Grass");
                    MapManager.Instance.SetMapRuleTile(fillerRightTile, 1, "TownDirt_To_Grass");

                    // Update tile data
                    leftTile.terrainType = ZetaUtilities.TERRAIN_SETTLEMENT_ROAD;
                    leftTile.walkable = true;
                    rightTile.terrainType = ZetaUtilities.TERRAIN_SETTLEMENT_ROAD;
                    rightTile.walkable = true;
                    fillerLeftTile.terrainType = ZetaUtilities.TERRAIN_SETTLEMENT_ROAD;
                    fillerLeftTile.walkable = true;
                    fillerRightTile.terrainType = ZetaUtilities.TERRAIN_SETTLEMENT_ROAD;
                    fillerRightTile.walkable = true;
                }

                // Update Astar graph for tile walkability
                ZetaUtilities.UpdateMultipleAstarGraphNodes(updatedTiles);
            }
        }
    }
}
