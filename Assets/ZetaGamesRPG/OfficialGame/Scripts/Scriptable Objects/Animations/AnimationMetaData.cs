using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {

    [CreateAssetMenu(menuName = "Animation Meta Data", fileName = "New Animation Meta Data", order = 51)]
    public class AnimationMetaData : ScriptableObject {

        // GENERAL ANIMATIONS

        public int idleAnimationLength;
        public float idleFramerate;
        public bool idleLoop;

        public int walkAnimationLength;
        public float walkFramerate;
        public bool walkLoop;

        public int deathAnimationLength;
        public float deathFramerate;
        public bool deathLoop;

        public int damageAnimationLength;
        public float damageFramerate;
        public bool damageLoop;

        public int chargeAttackAnimationLength;
        public float chargeAttackFramerate;
        public bool chargeAttackLoop;

        public int attackAnimationLength;
        public float attackFramerate;
        public bool attackLoop;

        // PROFESSION ANIMATION INFO

        public int loggingAnimationLength;
        public float loggingFramerate;
        public bool loggingLoop;

        public int miningAnimationLength;
        public float miningFramerate;
        public bool miningLoop;

        public int harvestingAnimationLength;
        public float harvestingFramerate;
        public bool harvestingLoop;

        public int woodWorkAnimationLength;
        public float woodWorkFramerate;
        public bool woodWorkLoop;

        public int jewelryWorkAnimationLength;
        public float jewelryWorkFramerate;
        public bool jewelryWorkLoop;

        public int blacksmithWorkAnimationLength;
        public float blacksmithWorkFramerate;
        public bool blacksmithWorkLoop;

        public int alchemyWorkAnimationLength;
        public float alchemyWorkFramerate;
        public bool alchemyWorkLoop;
    }
}
