using UnityEngine;
using UnityEngine.AI;

namespace ZetaGames.RPG {
    public abstract class AIBrain : MonoBehaviour {
        protected StateMachineMultiCondition stateMachine;
        public NavMeshAgent navMeshAgent { get; set; }
        public Animator animator { get; set; }
        public AnimationManager animationManager { get; set; }
        public bool haveDestination { get; set; }
        public GatherableResource resourceNodeTarget { get; set; }
        public NpcResourcePickup resourceDropTarget { get; set; }
        public Vector2 destination { get; set; }
        public StockPile stockPile { get; set; }
        public float timeStuck { get; set; }
        public Vector2 home { get; set; }
        public bool debugLogs;
        public bool atDestination { get; set; }
        public float wanderCooldown;
        public bool inCombat;

        private void Awake() {
            // Cache NPC components
            navMeshAgent = GetComponent<NavMeshAgent>();
            animator = GetComponentInChildren<Animator>();
            animationManager = GetComponentInChildren<AnimationManager>();

            // Create state machine for NPC
            stateMachine = new StateMachineMultiCondition();
            stateMachine.MonoParser(this);

            // Record "Home"
            home = transform.position;
        }

        private void Update() => stateMachine.Tick();

        public abstract bool TakeFromTarget();

        public abstract bool Take();

        public abstract void DropAllResources();

        public void DestroyObject(GameObject gameObject, float delay) {
            Destroy(gameObject, delay);
        }

        public abstract void ResetAgent();
    }
}

