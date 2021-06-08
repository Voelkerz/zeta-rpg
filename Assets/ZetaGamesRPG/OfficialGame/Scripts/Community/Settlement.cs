using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    public class Settlement {

        public class Region {
            public Region(int x, int y) {
                this.x = x;
                this.y = y;
                settled = false;
                northSettled = false;
                southSettled = false;
                westSettled = false;
                eastSettled = false;
                northExit = false;
                southExit = false;
                westExit = false;
                eastExit = false;
                numStructures = 0;
            }

            public int x { get; }
            public int y { get; }
            public bool settled;
            public bool northSettled;
            public bool southSettled;
            public bool westSettled;
            public bool eastSettled;
            public bool northExit;
            public bool southExit;
            public bool westExit;
            public bool eastExit;
            public int numStructures;
        }

        public string settlementName;
        public Region originRegion;
        public List<Region> growthRegions;
        public Dictionary<string, List<Vector3>> settlementEntrances;
        public Vector3 bulletinBoardPos;
        public Dictionary<string, List<Vector3>> publicBuildings;
        public List<WorldTile> boundaryWallTiles;
        public List<WorldTile> allSettlementTiles;
        public GameObject settlementLeader;
        public List<GameObject> citizenList;
        public int maxPopulation;
        public bool atMax;

        public Settlement(string settlementName, GameObject settlementLeader) {
            this.settlementLeader = settlementLeader;
            this.settlementName = settlementName;

            growthRegions = new List<Region>();
            settlementEntrances = new Dictionary<string, List<Vector3>>();
            publicBuildings = new Dictionary<string, List<Vector3>>();
            boundaryWallTiles = new List<WorldTile>();
            allSettlementTiles = new List<WorldTile>();
            citizenList = new List<GameObject>();
            citizenList.Add(settlementLeader);
        }

        public bool ContainsRegion(int x, int y) {
            foreach (Region localSettledRegion in growthRegions) {
                if (localSettledRegion.x == x && localSettledRegion.y == y) {
                    return true;
                }
            }

            return false;
        }

        public Region GetSettledRegion(int x, int y) {
            foreach (Region localSettledRegion in growthRegions) {
                if (localSettledRegion.x == x && localSettledRegion.y == y) {
                    return localSettledRegion;
                }
            }

            Debug.LogError("Settlement.GetSettledRegion(): No settled region found with coords (" + x + ", " + y + "). Returning origin region.");
            return originRegion;
        }

        public void SetSettledRegion(int x, int y) {
            for (int i = 0; i < growthRegions.Count; i++) {
                Region localRegion = growthRegions[i];

                if (localRegion.x == x && localRegion.y == y) {
                    localRegion.settled = true;

                    // Change the 4 neighboring regions around this one
                    if (ContainsRegion(x - 1, y)) {
                        Region westNeighbor = GetSettledRegion(x - 1, y);
                        westNeighbor.eastSettled = true;
                    }

                    if (ContainsRegion(x + 1, y)) {
                        Region eastNeighbor = GetSettledRegion(x + 1, y);
                        eastNeighbor.westSettled = true;
                    }

                    if (ContainsRegion(x, y - 1)) {
                        Region southNeighbor = GetSettledRegion(x, y - 1);
                        southNeighbor.northSettled = true;
                    }

                    if (ContainsRegion(x, y + 1)) {
                        Region northNeighbor = GetSettledRegion(x, y + 1);
                        northNeighbor.southSettled = true;
                    }

                    break;
                }
            }
        }
    }
}
