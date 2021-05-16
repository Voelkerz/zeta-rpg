using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System.Collections;

namespace ZetaGames.RPG {
    public abstract class AIBrain : MonoBehaviour {
        protected StateMachineMultiCondition stateMachine;
        [HideInInspector] public Personality personality { get; set; }
        [HideInInspector] public AIMovement pathMovement { get; set; }
        [HideInInspector] public Animator animator { get; set; }
        //[HideInInspector] public WorldTile resourceTileTarget { get; set; }
        [HideInInspector] public NpcMemory memory { get; set; }
        [HideInInspector] public NpcInventory inventory { get; set; }
        [HideInInspector] public BuildingManager buildingManager { get; set; }
        [HideInInspector] public NpcBuildGoal buildGoal { get; set; }
        [HideInInspector] public NpcStats stats { get; set; }
        [HideInInspector] public NpcNeeds needs { get; set; }
        [HideInInspector] public bool useAdvAI { get; set; }
        [HideInInspector] public float deltaTime { get; set; }
        [HideInInspector] public float wanderCooldown { get; set; }

        // NPC States
        public BuildStructure buildStructure;
        public SearchForResource searchForResource;
        public HarvestResource harvestResource;
        public StoreResource storeResource;
        public PickupItem pickupItem;
        public Wander wander;

        // Tools
        private WaitForSeconds[] waitTimers = new WaitForSeconds[3];
        private int npcLockTag;

        // EDITOR PROPERTIES
        public bool inCombat;
        public bool debugLogs;

        private void Awake() {
            // Cache NPC components
            pathMovement = GetComponent<AIMovement>();
            animator = GetComponentInChildren<Animator>();
            buildingManager = FindObjectOfType<BuildingManager>();

            // Create state machine for NPC
            stateMachine = new StateMachineMultiCondition();
            stateMachine.MonoParser(this);

            if (debugLogs) {
                stateMachine.debugLog = true;
            }

            // Create necessary parts for AI
            memory = new NpcMemory();
            buildGoal = new NpcBuildGoal(this);
            needs = new NpcNeeds(this);
            inventory = new NpcInventory();
            stats = new NpcStats();

            // Set unique NPC lock tag
            npcLockTag = gameObject.GetInstanceID();

            deltaTime = 0;
            useAdvAI = false;
            pathMovement.useSimplePathing = true;

            // Initialize commonly used wait timers
            waitTimers[0] = new WaitForSeconds(1f);
            waitTimers[1] = new WaitForSeconds(1f);
            waitTimers[2] = new WaitForSeconds(1f);
        }

        protected virtual void Update() {
            if (deltaTime > 1f) {
                stateMachine.Tick();
                updateCooldownTimers();
                evaluateNeeds();

                deltaTime = 0;
            }

            deltaTime += Time.deltaTime;
        }

        public virtual int GetNpcLockTag() {
            return npcLockTag;
        }

        public virtual void ResetAgent() {

        }

        protected virtual void evaluateNeeds() {
            // Shelter Need
            if (needs.CalculateShelterScore() < 100) {
                if (!buildGoal.planned) {
                    buildGoal.CreateShelterBuildGoal();
                }
            }

            // Community Need
            
            
            // Physiological Needs
            // - Food (will start with food)
            // - Energy (will start with full energy)
            // - Clothing (will start with basic clothing)
            // - Shelter = npcNeeds.EvaluateShelter();
            // - Health (will start with full health)

            // Safety Needs
            // - Security
            // - Employment
            // - Health
            // - Property
            // - Finances

            // Social Needs
            // - Family
            // - Intimacy
            // - Belonging to social group
        }

        protected virtual void updateCooldownTimers() {
            wanderCooldown += deltaTime;
        }

        public virtual void DestroyObject(GameObject gameObject, float delay) {
            Destroy(gameObject, delay);
        }

        public virtual GameObject InstantiateObject(GameObject gameObject, Vector3 position) {
            return Instantiate(gameObject, position, Quaternion.identity);
        }

        public virtual void SetTilemapSpriteAsync(WorldTile worldTile, int tilemapIndex, string spriteName) {
            StartCoroutine(MapManager.Instance.SetAtlasedSpriteAsync(worldTile, tilemapIndex, spriteName));
        }

        public virtual void PlayResourceSpriteAnimation(WorldTile worldTile, int tilemapIndex, List<string> spriteNames, int startIndex) {
            StartCoroutine(MapManager.Instance.PlayResourceSpriteAnimation(worldTile, tilemapIndex, spriteNames, startIndex));
        }

        public virtual void OnVisible() {
            useAdvAI = true;
            pathMovement.useSimplePathing = false;
        }

        public virtual void OnInvisible() {
            useAdvAI = false;
            pathMovement.useSimplePathing = true;
        }

        public virtual void UnloadResources() {
            int randIndex = Random.Range(0, waitTimers.Length);
            StartCoroutine(TakeTimeToUnload(randIndex));
        }

        private IEnumerator TakeTimeToUnload(int randIndex) {
            yield return waitTimers[randIndex];
            //inventory.DropResource();
        }
    }
}

