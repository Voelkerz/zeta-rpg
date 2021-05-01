using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    public enum NodeType {
        Oak
    }

    [CreateAssetMenu(menuName = "Resource Node Data", fileName = "New Resource Node Data", order = 51)]
    public class ResourceNodeData : BaseObject {
        public NodeType type;
        public ResourceType resourceType;
        public GameObject resourceLoot;
        public int maxHitPoints;

        // extra grid tiles the tree occupies due to a large size
        public List<Vector3Int> adjacentGridOccupation;

        // the four tiles around the tree the character can harvest from
        public List<Vector3> harvestSpots;
    }
}
