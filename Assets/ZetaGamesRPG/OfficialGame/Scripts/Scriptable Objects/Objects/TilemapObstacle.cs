using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    public class TilemapObstacle : ScriptableObject {

        public string spriteName;
        public string spriteShadowName;
        public bool walkable;
        // extra grid tiles the obstacle occupies due to a large size
        public List<Vector3Int> additionalGridOccupation = new List<Vector3Int>();
    }
}
