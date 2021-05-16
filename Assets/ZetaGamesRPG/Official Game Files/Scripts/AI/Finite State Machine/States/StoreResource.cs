using UnityEngine;

namespace ZetaGames.RPG {
    public class StoreResource : State {
        public override int priority => 5;
        public override bool isFinished { get => finished; }
        public override bool isInterruptable { get => true; }
        
        private bool finished;
        private AIBrain npcBrain;
        private Vector3 stockpilePos;

        public StoreResource(AIBrain npcBrain) {
            this.npcBrain = npcBrain;
        }

        public override void OnEnter() {
            if (npcBrain.debugLogs) {
                Debug.Log("StoreResource.OnEnter(): Unloading resources.");
            }
            //npcBrain.resourceTileTarget = null;
            finished = false;
            stockpilePos = (Vector3)npcBrain.memory.RetrieveMemory("home");
            npcBrain.pathMovement.destination = stockpilePos;
            npcBrain.pathMovement.SearchPath();
        }

        public override void OnExit() {
            if (npcBrain.debugLogs) {
                Debug.Log("StoreResource.OnExit(): Resources unloaded.");
            }
        }

        public override void Tick() {
            if (!finished) {
                if (npcBrain.pathMovement.isStopped && Vector3.Distance(npcBrain.transform.position, stockpilePos) < 2f) {
                    if (npcBrain.debugLogs) {
                        Debug.Log("StoreResource.Tick(): Waiting to unload resources");
                    }
                    npcBrain.UnloadResources();
                    finished = true;
                }
            }
        }
    }
}

