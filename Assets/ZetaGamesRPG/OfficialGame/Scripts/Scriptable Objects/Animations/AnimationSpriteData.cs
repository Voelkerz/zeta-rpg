using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {

    [CreateAssetMenu(menuName = "Animation Sprite Data", fileName = "New Animation Sprite Data", order = 51)]
    public class AnimationSpriteData : ScriptableObject {

        // GENERAL ANIMATION SPRITES

        public Sprite[] idle;
        public Sprite[] walk;
        public Sprite[] death;
        public Sprite[] damage;
        public Sprite[] chargeAttack;
        public Sprite[] attack;

        // PROFESSION ANIMATION SPRITES

        public Sprite[] logging;
        public Sprite[] mining;
        public Sprite[] harvesting;
        public Sprite[] woodWork;
        public Sprite[] jewelryWork;
        public Sprite[] blacksmithWork;
        public Sprite[] alchemyWork;
    }
}
