using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    public class NpcNeeds {

        private readonly AIBrain npc;

        // Physiological Needs
        public int food;
        public float rest;
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

        public float CalculateRestScore() {
            rest = 100;

            if (true) {
                rest -= 100 / 288;
            }

            return rest;
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

        public int CalculateShelterScore() {
            shelter = 0;
            // Does NPC have a home?
            if (npc.stats.homePropertyData != null) {
                // Has home
                // Is NPC far from home? (300 tiles away should add 60 points)
                shelter += (int)Mathf.Clamp(Vector3.Distance((Vector3)npc.memory.RetrieveMemory(ZetaUtilities.MEMORY_LOCATION_HOME), npc.transform.position) / 5, 0f, 100f);
            } else if (npc.stats.homePropertyData == null) {
                // No Home
                shelter = 100;
            }
            return shelter;
        }
    }
}
