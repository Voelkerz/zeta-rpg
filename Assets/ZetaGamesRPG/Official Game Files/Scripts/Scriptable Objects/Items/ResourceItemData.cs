using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    [CreateAssetMenu(menuName = "Resource", fileName = "New Resource", order = 51)]
    public class ResourceItemData : BaseItem {

        [SerializeField] private ResourceCategory resourceType;

        public ResourceCategory GetResourceType() {
            return resourceType;
        }
    }
}

