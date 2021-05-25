using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    public class BuildingManager : MonoBehaviour {

        public static BuildingManager Instance;
        public List<BaseStructureData> buildableStructures;

        private void Awake() {
            Instance = this;
        }
    }
}
