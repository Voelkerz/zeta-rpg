using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    [CreateAssetMenu(menuName = "Resource", fileName = "New Resource", order = 51)]
    public class ResourceData : BaseItem {

        [SerializeField] private ResourceType resourceType;
    }
}

