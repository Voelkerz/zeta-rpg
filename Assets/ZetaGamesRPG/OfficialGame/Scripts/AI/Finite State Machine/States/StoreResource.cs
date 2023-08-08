using UnityEngine;

namespace ZetaGames.RPG {
    public class StoreResource : State {
        public override float actionScore { get => 5; set => actionScore = value; }
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
            stockpilePos = npcBrain.transform.position + new Vector3(10, 10);
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

        public override float GetUtilityScore() {
            throw new System.NotImplementedException();
        }

        public override void AddUtilityScore(float amount) {
            throw new System.NotImplementedException();
        }
    }
}

