using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    [CreateAssetMenu(menuName = "Resource Node Data", fileName = "New Resource Node Data", order = 51)]
    public class ResourceNode : TilemapObstacle {
        public ResourceItem resourceItemData;
        public string spriteFull;
        public string spriteFullShadow;
        public string spriteDepleted;
        public string spriteDepletedShadow;
        public List<string> spriteAnimationList;
        public ResourceType resourceType;
        public ResourceCategory resourceCategory;
        public int lootPerDrop;
        public int maxLoot;
        public int maxHitPoints;

        // the four tiles around the node the character can harvest from
        public List<Vector3> harvestPositions;
    }
}
