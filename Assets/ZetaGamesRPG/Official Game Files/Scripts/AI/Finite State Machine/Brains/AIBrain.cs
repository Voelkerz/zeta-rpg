using UnityEngine;
using UnityEngine.AI;
using Pathfinding;

namespace ZetaGames.RPG {
    public abstract class AIBrain : MonoBehaviour {
        protected StateMachineMultiCondition stateMachine;
        [HideInInspector] public Personality personality { get; set; }
        //[HideInInspector] public NavMeshAgent navMeshAgent { get; set; }
        [HideInInspector] public AIMovement pathAgent;
        [HideInInspector] public MapManager mapManager;
        [HideInInspector] public Animator animator { get; set; }
        [HideInInspector] public AnimationManager animationManager { get; set; }
        [HideInInspector] public WorldTile resourceTileTarget { get; set; }
        [HideInInspector] public NpcMemory npcMemory { get; set; }
        [HideInInspector] public NpcInventory npcInventory { get; set; }
        [HideInInspector] public Vector3 destinationPos { get; set; }
        [HideInInspector] public float timeStuck { get; set; }
        [HideInInspector] public float wanderCooldown { get; set; }
        [HideInInspector] public ResourceType resourceTypeWanted { get; set; }
        [HideInInspector] public float accumulatedTimeDelta;
        [HideInInspector] public bool useAdvAI { get; set; }
        private float updateTimer;

        // EDITOR PROPERTIES
        public bool inCombat;
        public bool debugLogs;

        private void Awake() {
            // Cache NPC components
            //navMeshAgent = GetComponent<NavMeshAgent>();
            pathAgent = GetComponent<AIMovement>();
            //destinationSetter = GetComponent<AIDestinationSetter>();
            animator = GetComponentInChildren<Animator>();
            animationManager = GetComponentInChildren<AnimationManager>();
            npcInventory = GetComponent<NpcInventory>();

            // Create state machine for NPC
            stateMachine = new StateMachineMultiCondition();
            stateMachine.MonoParser(this);

            // Create a memory for npc
            npcMemory = new NpcMemory();

            // Cache the map manager
            mapManager = FindObjectOfType<MapManager>();

            // Set update timer to zero
            updateTimer = 0;
        }

        protected virtual void Update() {
            // timer helps slow down AI so it's not making 60+ calculations a second
            //if (updateTimer > 0.1f) {
                updateCooldownTimers();
                updateNeeds();
                stateMachine.Tick();
                updateTimer = 0;
                accumulatedTimeDelta = 0;
            //} else {
                updateTimer = Time.deltaTime;

                // Accumulate delta time for accurate timer-based updates
                accumulatedTimeDelta = Time.deltaTime;
            //}

            // TODO: write code to change AI complexity depending on whether offscreen or not
            useAdvAI = true;
        }

        protected virtual void updateNeeds() {
            
        }

        protected virtual void updateCooldownTimers() {
            wanderCooldown += Time.deltaTime;
        }

        public virtual void DestroyObject(GameObject gameObject, float delay) {
            Destroy(gameObject, delay);
        }

        public virtual void ResetAgent() {
            
        }
    }
}

