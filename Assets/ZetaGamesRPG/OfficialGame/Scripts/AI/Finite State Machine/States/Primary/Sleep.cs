using UnityEngine;

namespace ZetaGames.RPG {
    public class Sleep : State {
        public override float actionScore { get => 50; set => actionScore = value; }
        public override bool isFinished => finished;
        public override bool isInterruptable => npc.inCombat;

        public enum SleepState {
            Rested,
            Tired,
            VeryTired,
            Exhausted
        }

        private bool finished;
        private readonly AIBrain npc;
        public SleepState currentState;
        public bool awake = true;
        private Vector3 bedPosition;
        private Vector3 destination;

        public Sleep(AIBrain npc) {
            this.npc = npc;
            currentState = SleepState.Rested;
        }

        public override void OnEnter() {
            if (npc.debugLogs) Debug.Log("I'm tired, looking for a place to rest.");
            
            if (npc.stats.homePropertyData != null) {
                bedPosition = (Vector3)npc.memory.RetrieveMemory(ZetaUtilities.MEMORY_LOCATION_HOME);
                bedPosition.x += npc.stats.homePropertyData.doorTile.x;
                bedPosition.y += npc.stats.homePropertyData.doorTile.y;

                if (bedPosition != null) {
                    destination = bedPosition;
                    npc.pathMovement.destination = destination;
                    npc.pathMovement.SearchPath();
                } else {
                    Debug.LogError("Sleep.OnEnter(): Bed position is null.");
                    finished = true;
                }
            } else {
                Debug.LogError("Sleep.OnEnter(): No home property data.");
                finished = true;
            }
        }

        public override void OnExit() {
            awake = true;
            finished = false;
        }

        public override void Tick() {
            if (!finished) {
                if (awake) {
                    if (npc.pathMovement.isStopped && Vector3.Distance(npc.transform.position, destination) <= 2f) {
                        if (npc.debugLogs) Debug.Log("Resting now.");
                        awake = false;
                    }
                } else if (npc.restScore >= 100 && TimeManager.Instance.currentTick > TimeManager.Instance.numTicksPerDay / 4.8) {
                    // 5am in-game time is ticks divided 4.8
                    if (npc.debugLogs) Debug.Log("Done resting!");
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
