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
            shelter = 100;
            
            // Does NPC have a home?
            if (npc.stats.homeProperty != null) {
                // Yes

            } else {
                // No
                shelter -= 100;
            }

            // Return calculated score
            if (shelter < 0) {
                shelter = 0;
            }

            return shelter;
        }
    }
}
