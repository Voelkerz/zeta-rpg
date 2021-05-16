using UnityEngine;

namespace ZetaGames.RPG {
    public class Wander : State {
        public override int priority => 0;
        public override bool isFinished => finished;
        public override bool isInterruptable => true; //{ get => npcBrain.inCombat || cycleCount > 1; } // state will not be interrupted until specified full 'wandering' cycles are finished (unless combat is initiated)
        

        private bool finished;
        private AIBrain npcBrain;
        public float wanderRadius;
        public float wanderCycle;
        private float tickTimer;
        private int cycleCount;
        Vector3 destination;

        public Wander(AIBrain npcBrain) {
            this.npcBrain = npcBrain;
            wanderRadius = npcBrain.personality.wanderRadius;
            wanderCycle = npcBrain.personality.wanderCycle;
        }

        public override void Tick() {
            tickTimer += npcBrain.deltaTime;

            if (tickTimer >= wanderCycle) {
                if (npcBrain.debugLogs) {
                    Debug.Log("Wander.Tick(): Just wandering, nothing better to do...");
                }

                if (npcBrain.pathMovement.isStopped && Vector3.Distance(npcBrain.transform.position, destination) <= 1f) {
                    destination = new Vector3(npcBrain.transform.position.x + Random.Range(-wanderRadius, wanderRadius), npcBrain.transform.position.x + Random.Range(-wanderRadius, wanderRadius));
                    npcBrain.pathMovement.destination = destination;
                    npcBrain.pathMovement.SearchPath();
                    tickTimer = 0;
                    cycleCount++;
                }
            }
        }

        public override void OnEnter() {
            //npcBrain.ResetAgent();
            tickTimer = wanderCycle;
            cycleCount = 0;
            destination = npcBrain.transform.position;
            npcBrain.wanderCooldown = 0;
        }

        public override void OnExit() {
           
        }
    }
}

