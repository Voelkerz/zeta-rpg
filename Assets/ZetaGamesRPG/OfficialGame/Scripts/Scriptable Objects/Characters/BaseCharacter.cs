using UnityEngine;

namespace ZetaGames.RPG {

    [CreateAssetMenu(menuName = "Base Character Data", fileName = "New Base Character Data", order = 51)]
    public class BaseCharacter : ScriptableObject {

        // Animation Data
        public AnimationMetaData animMeta;
        public AnimationSpriteData bodyAnimations;
        public AnimationSpriteData shadowAnimations;
    }
}
