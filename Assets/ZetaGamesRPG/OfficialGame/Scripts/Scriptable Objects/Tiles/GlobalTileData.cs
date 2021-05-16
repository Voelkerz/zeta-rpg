using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ZetaGames.RPG {
    [CreateAssetMenu(menuName = "Global Tile Data", fileName = "New Global Tile Data", order = 51)]
    public class GlobalTileData : ScriptableObject {

        public TileBase[] tiles;

        public float pathPenalty;
        public float speedPercent;
        public bool walkable;
        public string type;
        public bool animated;
    }
}

