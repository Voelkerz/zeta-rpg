using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ZetaGames.RPG {
    public abstract class AIBrain : MonoBehaviour {
        protected StateMachineMultiCondition stateMachine;
        public Personality personality { get; set; }
        public NavMeshAgent navMeshAgent { get; set; }
        public Animator animator { get; set; }
        public AnimationManager animationManager { get; set; }
        //public GatherableResource resourceNodeTarget { get; set; }
        public GameObject resourceTarget { get; set; }
        public NpcMemory npcMemory { get; set; }
        public NpcInventory npcInventory { get; set; }
        public Vector2 destination { get; set; }
        public float timeStuck { get; set; }
        public float wanderCooldown { get; set; }
        public ResourceType resourceNeeded { get; set; }

        // EDITOR PROPERTIES
        public bool inCombat;
        public bool debugLogs;

        private void Awake() {
            // Cache NPC components
            navMeshAgent = GetComponent<NavMeshAgent>();
            animator = GetComponentInChildren<Animator>();
            animationManager = GetComponentInChildren<AnimationManager>();
            npcInventory = GetComponent<NpcInventory>();
            //var enemyDetector = gameObject.AddComponent<EnemyDetector>();
            //var fleeParticleSystem = gameObject.GetComponentInChildren<ParticleSystem>();

            // Create state machine for NPC
            stateMachine = new StateMachineMultiCondition();
            stateMachine.MonoParser(this);

            // Create a memory for npc
            npcMemory = new NpcMemory();
        }

        protected virtual void Update() {
            stateMachine.Tick();
            updateCooldownTimers();
        }

        protected virtual void updateCooldownTimers() {
            wanderCooldown += Time.deltaTime;
        }

        public virtual void DestroyObject(GameObject gameObject, float delay) {
            Destroy(gameObject, delay);
        }

        public virtual void ResetAgent() {
            animator.SetBool("move", false);
        }
    }
}

