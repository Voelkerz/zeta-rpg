using UnityEngine;
using UnityEngine.AI;

namespace ZetaGames.RPG {
    internal class GetUnstuck : IState {
        public bool isFinished { get => finished; }
        public bool isInterruptable { get => timeInState > 10f; }
        private bool finished;
        private AIBrain npcBrain;
        //private NavMeshAgent navMeshAgent;
        private readonly AnimationManager animationManager;
        private Vector3 lastPosition = Vector3.zero;
        private float timeInState;

        public GetUnstuck(AIBrain npcBrain) {
            this.npcBrain = npcBrain;
            //navMeshAgent = npcBrain.navMeshAgent;
            animationManager = npcBrain.animationManager;
        }

        public void Tick() {
            timeInState += Time.deltaTime;

            //animationManager.Move();
        }

        public void OnEnter() {
            npcBrain.timeStuck = 0f;
            timeInState = 0;
            npcBrain.ResetAgent();
            npcBrain.timeStuck = 0f;
            npcBrain.resourceTileTarget = null;
            npcBrain.destinationPos = (Vector3)npcBrain.npcMemory.RetrieveMemory("home");
            //navMeshAgent.destination = npcBrain.destinationPos;
        }

        public void OnExit() {
            
        }
    }
}