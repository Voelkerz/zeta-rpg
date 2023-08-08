using UnityEngine;
using UnityEngine.AI;

namespace ZetaGames.RPG {
    internal class GetUnstuck : State {
        public override float actionScore { get => 0; set => actionScore = value; }
        public override bool isFinished { get => finished; }
        public override bool isInterruptable { get => timeInState > 10f; }

        

        private bool finished;
        private AIBrain npcBrain;
        //private NavMeshAgent navMeshAgent;
        //private readonly AnimationManager animationManager;
        private Vector3 lastPosition = Vector3.zero;
        private float timeInState;

        public GetUnstuck(AIBrain npcBrain) {
            this.npcBrain = npcBrain;
            //navMeshAgent = npcBrain.navMeshAgent;
            //animationManager = npcBrain.animationManager;
        }

        public override void Tick() {
            timeInState += Time.deltaTime;

            //animationManager.Move();
        }

        public override void OnEnter() {
            //npcBrain.timeStuck = 0f;
            timeInState = 0;
            npcBrain.ResetAgent();
            //npcBrain.timeStuck = 0f;
            //npcBrain.resourceTileTarget = null;
            //npcBrain.destinationPos = (Vector3)npcBrain.npcMemory.RetrieveMemory("home");
            //navMeshAgent.destination = npcBrain.destinationPos;
        }

        public override void OnExit() {
            
        }

        public override float GetUtilityScore() {
            throw new System.NotImplementedException();
        }

        public override void AddUtilityScore(float amount) {
            throw new System.NotImplementedException();
        }
    }
}