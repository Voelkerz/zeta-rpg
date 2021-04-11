using UnityEngine;
using UnityEngine.AI;

namespace ZetaGames.RPG {
    internal class MoveToResource : IState {
        public bool isFinished { get => finished; }
        public bool isInterruptable { get => true; }
        private bool finished;
        private readonly AIBrain npcBrain;
        private NavMeshAgent navMeshAgent;
        private readonly AnimationManager animationManager;
        private Vector3 lastPosition = Vector3.zero;

        public MoveToResource(AIBrain npcBrain) {
            this.npcBrain = npcBrain;
            navMeshAgent = npcBrain.navMeshAgent;
            animationManager = npcBrain.animationManager;
        }

        public void Tick() {
            if (Vector2.Distance(npcBrain.transform.position, lastPosition) <= 0f) {
                npcBrain.timeStuck += Time.deltaTime;
            }
            lastPosition = npcBrain.transform.position;
            animationManager.Move();
        }

        public void OnEnter() {
            npcBrain.timeStuck = 0f;
            navMeshAgent.destination = npcBrain.destination;
        }

        public void OnExit() {
            
        }
    }
}

