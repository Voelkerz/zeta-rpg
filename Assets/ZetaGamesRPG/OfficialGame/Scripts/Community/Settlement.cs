using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    public class Settlement {

        public string settlementName;
        public WorldTile[] homeRegion;
        public Vector3Int homeRegionPos;
        public Dictionary<Vector3Int, WorldTile[]> neighboringRegions;
        public GameObject settlementLeader;
        public List<GameObject> citizenList;
        public int maxPopulation;

        public Settlement(string settlementName, GameObject settlementLeader) {
            this.settlementLeader = settlementLeader;
            this.settlementName = settlementName;

            neighboringRegions = new Dictionary<Vector3Int, WorldTile[]>();
            citizenList = new List<GameObject>();
            citizenList.Add(settlementLeader);
        }
    }
}
