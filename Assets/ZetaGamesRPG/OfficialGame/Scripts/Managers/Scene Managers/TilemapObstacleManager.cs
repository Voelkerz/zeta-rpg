using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    public class TilemapObstacleManager : MonoBehaviour {

        public static TilemapObstacleManager
            Instance;
        public List<BaseStructureData> buildableStructures;
        public List<MapObstacle> tilemapObstacles;

        private void Awake() {
            Instance = this;
        }
    }
}
