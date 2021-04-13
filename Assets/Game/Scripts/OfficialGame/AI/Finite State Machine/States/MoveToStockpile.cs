using UnityEngine;
using UnityEngine.AI;

namespace ZetaGames.RPG {
    internal class MoveToStockpile {
        /*
        public bool isFinished { get => finished; }
        public bool isInterruptable { get => true; }
        private bool finished;
        private readonly AIBrain npcBrain;
        private NavMeshAgent navMeshAgent;
        private readonly AnimationManager animationManager;
        private Vector3 lastPosition = Vector3.zero;
        private static readonly int shouldMove = Animator.StringToHash("move");

        public MoveToStockpile(AIBrain npcBrain) {
            this.npcBrain = npcBrain;
            navMeshAgent = npcBrain.navMeshAgent;
            animationManager = npcBrain.animationManager;
            npcBrain.stockPile = Object.FindObjectOfType<StockPile>();
        }

        public void Tick() {
            if (Vector3.Distance(npcBrain.transform.position, lastPosition) <= 0f) {
                npcBrain.timeStuck += Time.deltaTime;
            }

            lastPosition = npcBrain.transform.position;

            animationManager.Move();
        }

        public void OnEnter() {
            npcBrain.timeStuck = 0f;
            navMeshAgent.destination = ZetaUtilities.FindNearestEdgeOfCollider(npcBrain.transform.position, npcBrain.stockPile.gameObject.GetComponent<Collider2D>());
        }

        public void OnExit() {
            npcBrain.animator.SetBool(shouldMove, false);
        }
        */
    }
}

