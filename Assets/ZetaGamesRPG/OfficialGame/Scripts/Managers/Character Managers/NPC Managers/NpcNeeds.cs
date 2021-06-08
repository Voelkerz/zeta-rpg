using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    public class NpcNeeds {

        private readonly AIBrain npc;

        // Physiological Needs
        public int food;
        public int rest;
        public int clothing;
        public int shelter;
        public int health;

        // Safety Needs
        public int community;
        public int employment;
        public int wellbeing;
        public int property;
        public int financial;

        public NpcNeeds(AIBrain npc) {
            this.npc = npc;
        }

        public int CalculateShelterScore() {
            shelter = 0;
            // Does NPC have a home?
            if (npc.stats.homeProperty != null) {
                // Has home
                // Is NPC far from home? (300 tiles away should add 60 points)
                shelter += (int)Mathf.Clamp(Vector3.Distance((Vector3)npc.memory.RetrieveMemory(ZetaUtilities.MEMORY_LOCATION_HOME), npc.transform.position) / 5, 0f, 100f);
            } else if (npc.stats.homeProperty == null) {
                // No Home
                shelter = 100;
            }
            return shelter;
        }

        public int CalculateCommunityScore() {
            /*************************************************************
             * Community score will be used for pushing community related
             * actions. For citizens it cause them to make demands to the
             * leader. For the leader, demands will cause the score to move
             * and eventually take action.
            *************************************************************/
            community = 0;

            // Does NPC belong to a community?
            if (npc.joinCommunity.hasCommunity) {
                
            } else {
                community = 100;
            }

            return community;
        }
    }
}
