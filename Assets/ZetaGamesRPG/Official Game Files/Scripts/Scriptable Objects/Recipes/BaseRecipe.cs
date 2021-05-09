using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    [CreateAssetMenu(menuName = "Recipe", fileName = "New Recipe", order = 51)]
    public class BaseRecipe : ScriptableObject {
        public BaseItemData[] ingredients;
        public BaseItemData successReward;

        [Range(0, 100)]
        public int successChance = 65;
        public BaseItemData failReward;
    }
}

