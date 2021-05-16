using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    [CreateAssetMenu(menuName = "Resource Drop Data", fileName = "New Resource Drop Data", order = 51)]
    public class ResourceItem : BaseItem {

        public ResourceState resourceState;
        public ResourceType resourceType;
        public ResourceCategory resourceCategory;
    }
}

