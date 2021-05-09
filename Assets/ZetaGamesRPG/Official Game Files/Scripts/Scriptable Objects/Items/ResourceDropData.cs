using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    [CreateAssetMenu(menuName = "Resource Drop", fileName = "New Resource Drop", order = 51)]
    public class ResourceDropData : BaseItemData {

        public ResourceState resourceState;
        public ResourceType resourceType;
        public ResourceCategory resourceCategory;
    }
}

