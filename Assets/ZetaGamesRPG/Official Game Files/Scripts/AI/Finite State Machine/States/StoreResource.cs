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
            if (npcBrain.debugLogs) {
                Debug.Log("StoreResource.OnEnter(): Unloading resources.");
            }
            npcBrain.resourceTileTarget = null;
            npcBrain.UnloadResources();
        }

        public void OnExit() {
            if (npcBrain.debugLogs) {
                Debug.Log("StoreResource.OnExit(): Resources unloaded.");
            }
        }

        public void Tick() {
           
        }
    }
}

