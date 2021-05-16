using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    [CreateAssetMenu(menuName = "Structure Data", fileName = "New Structure Data", order = 51)]
    public class BaseStructureData : BaseObject {
        // General Data
        public GameObject prefab;
        public EconomicClass quality;
        public StructureCategory category;
        public StructureType type;

        // Tilemap Data
        public int sizeX;
        public int sizeY;
        public List<Vector3> wallBoundary;
        public List<Vector3> walkableTiles;
        public List<Vector3> blockedTiles;
        public Vector3 doorTile;
        
        // Building Costs
        public ResourceCategory material1;
        public int material1Amount;
        public ResourceCategory material2;
        public int material2Amount;
    }
}

