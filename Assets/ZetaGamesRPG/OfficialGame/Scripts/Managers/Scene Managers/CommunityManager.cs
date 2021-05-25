using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ZetaGames.RPG {
    public class CommunityManager : MonoBehaviour {

        public static CommunityManager Instance;
        public List<Settlement> settlementList;
        public int smallSettlementSize { get => 10; }
        public Dictionary<Vector3Int, WorldTile[]> viableRegions;
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
            viableRegions = new Dictionary<Vector3Int, WorldTile[]>();
        }

        private void Start() {
            // Determine viable regions to build settlements
            ZetaGrid<WorldTile[]> regionGrid = MapManager.Instance.GetWorldRegionGrid();
            Vector3Int regionPos = new Vector3Int(0, 0, 0);
            int mapWidth = MapManager.Instance.mapWidth;
            int mapHeight = MapManager.Instance.mapHeight;
            int regionSize = MapManager.Instance.regionSize;

            for (int regionX = 0; regionX < mapWidth / regionSize; regionX++) {
                for (int regionY = 0; regionY < mapHeight / regionSize; regionY++) {
                    WorldTile[] regionTileList = regionGrid.GetGridObject(regionX, regionY);
                    float numGround = 0;

                    foreach (WorldTile tile in regionTileList) {
                        if (tile.terrainType.Equals(ZetaUtilities.TERRAIN_GRASS)) {
                            numGround++;
                        }
                    }

                    // If number of ground tiles is over 80%
                    if ((numGround / regionTileList.Length) >= 0.8f) {
                        regionPos.x = regionX;
                        regionPos.y = regionY;
                        viableRegions.Add(regionPos, regionTileList);
                    }
                }
            }

            Debug.Log("Viable Regions: " + viableRegions.Count);
        }

        public Settlement JoinRandomSettlement(GameObject citizen) {
            Settlement settlement = settlementList[Random.Range(0, settlementList.Count)];
            settlement.citizenList.Add(citizen);
            return settlement;
        }

        public Settlement CreateSettlement(GameObject leader) {
            // Choose random name for settlement
            string settlementName = humanSettlementNames[Random.Range(0, humanSettlementNames.Count)];
            humanSettlementNames.Remove(settlementName);

            // Create settlement with leader and name
            Settlement settlement = new Settlement(settlementName, leader);
            settlement.maxPopulation = smallSettlementSize;

            // Pick a random region out of the list of viable regions to settle
            Vector3Int[] keyArray = new Vector3Int[viableRegions.Count];
            viableRegions.Keys.CopyTo(keyArray, 0);
            Vector3Int randKey = keyArray[Random.Range(0, keyArray.Length)];
            settlement.homeRegion = viableRegions[randKey];
            settlement.homeRegionPos = randKey;

            // Remove chosen region from the viable list
            viableRegions.Remove(randKey);

            // DEBUG ONLY!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            Tilemap tilemap = MapManager.Instance.tileMapList[1];
            Vector3Int tilePos = new Vector3Int(0, 0, 0);
            foreach (WorldTile tile in settlement.homeRegion) {
                tilePos.x = tile.x;
                tilePos.y = tile.y;
                tilemap.SetTile(tilePos, null);
            }
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            for (int x = 0; x < 7; x++) {
                for (int y = 0; y < 7; y++) {
                    Vector3Int regionKey = new Vector3Int(randKey.x + (x - 3), randKey.y + (y - 3), 0);

                    // Remove all neighboring from the viable list to space out settlements
                    if (viableRegions.ContainsKey(regionKey)) { 
                        viableRegions.Remove(regionKey);
                    }

                    // Add neighboring regions to settlement
                    if (MapManager.Instance.GetWorldRegionGrid().IsWithinGridBounds(regionKey.x, regionKey.y)) {
                        settlement.neighboringRegions.Add(regionKey, MapManager.Instance.GetWorldRegionGrid().GetGridObject(regionKey));
                    }
                }
            }

            // Add to total settlement list
            settlementList.Add(settlement);

            return settlement;
        }
    }
}
