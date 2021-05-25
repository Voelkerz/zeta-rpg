using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    [CreateAssetMenu(menuName = "Equippable Item Data", fileName = "New Equippable Item Data", order = 51)]
    public class EquippableItem : BaseItem {

        public AnimationSpriteData animationData;
        public Color itemColor;
    }
}
