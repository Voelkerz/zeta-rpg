using System.Collections;
using System.Collections.Generic;

namespace ZetaGames.RPG {
    public enum PersonalityType {
        Default
    }

    public class Personality {
        public float wanderMinCooldown;
        public float wanderRadius;
        public float wanderCycle;
        public float resourceMaxSearchTime;
        public float resourceMaxSearchRange;
        public float resourceMaxWanderRange;
        public int maxDistanceFromPosition;
        public float startCommunityChance;

        public Personality(PersonalityType type) {
            switch (type) {
                case PersonalityType.Default:
                    wanderMinCooldown = 10f; // wander cooldown timer in seconds
                    wanderRadius = 10f; // 10 unit walking range around position
                    wanderCycle = 10f; // 10 second delay before choosing another wander position
                    resourceMaxSearchTime = 1f; // 1 second of resource search uninterruptable
                    resourceMaxSearchRange = 10f; // 10 unit range around current position to look for resources
                    resourceMaxWanderRange = 25f; // range for walking and searching for a resource
                    maxDistanceFromPosition = 100; // a max distance to consider to keep from doing actions too far away (in grid tiles)
                    startCommunityChance = 50f;
                    break;
                default:
                    break;
            }
        }

        public Personality() {
            new Personality(PersonalityType.Default);
        }
    }
}

