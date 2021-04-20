using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    public class StoreResource : IState {
        public bool isFinished { get => finished; }
        public bool isInterruptable { get => true; }
        private bool finished;
        private AIBrain npcBrain;

        public StoreResource(AIBrain npcBrain) {
            this.npcBrain = npcBrain;
        }

        public void OnEnter() {
            
        }

        public void OnExit() {
            
        }

        public void Tick() {
           
        }
    }
}

