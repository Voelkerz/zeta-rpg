using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System.Collections;

namespace ZetaGames.RPG {
    public abstract class AIBrain : MonoBehaviour {
        protected StateMachineMultiCondition stateMachine;
        [HideInInspector] public Personality personality { get; set; }
        [HideInInspector] public AIMovement pathMovement { get; set; }
        [HideInInspector] public AnimationController animationController { get; set; }
        [HideInInspector] public CharacterInfo info { get; set; }
        [HideInInspector] public NpcMemory memory { get; set; }
        [HideInInspector] public NpcInventory inventory { get; set; }
        [HideInInspector] public NpcBuildGoal buildGoal { get; set; }
        [HideInInspector] public NpcStats stats { get; set; }
        [HideInInspector] public NpcNeeds needs { get; set; }
        [HideInInspector] public bool useAdvAI { get; set; }
        [HideInInspector] public float deltaTime { get; set; }
        [HideInInspector] public float wanderCooldown { get; set; }
        [HideInInspector] public int lockTag { get => gameObject.GetInstanceID(); }

        // NPC States
        public BuildStructure buildStructure;
        public SearchForResource searchForResource;
        public HarvestResource harvestResource;
        public StoreResource storeResource;
        public PickupItem pickupItem;
        public Wander wander;

        // Tools
        private WaitForSeconds[] waitTimers = new WaitForSeconds[3];
        private float needsEvalTimer;
        
        // EDITOR PROPERTIES
        public bool inCombat;
        public bool debugLogs;

        private void Awake() {
            // Cache NPC components
            pathMovement = GetComponent<AIMovement>();
            animationController = GetComponent<AnimationController>();
            info = GetComponent<CharacterInfo>();

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

            deltaTime = 0;
            useAdvAI = false;
            pathMovement.useSimplePathing = true;

            // Initialize commonly used wait timers
            waitTimers[0] = new WaitForSeconds(5f);
            waitTimers[1] = new WaitForSeconds(5f);
            waitTimers[2] = new WaitForSeconds(5f);
        }

        protected virtual void Update() {
            deltaTime += Time.deltaTime;
            needsEvalTimer += Time.deltaTime;

            if (deltaTime > 1f) {
                stateMachine.Tick();
                updateCooldownTimers();
                deltaTime = 0;

                if (needsEvalTimer > 5f) {
                    evaluateNeeds();
                }
            }
        }

        protected virtual void evaluateNeeds() {
            int communityScore = needs.CalculateCommunityScore();
            int shelterScore = needs.CalculateShelterScore();

            // Community Need
            if (communityScore > 0) {
                // do stuff
            }

            // Shelter Need
            if (shelterScore == 101) {
                // Special score. NPC needs a house.
                if (!buildGoal.hasBuildGoal) {
                    //TODO: More logic behind what house the NPC builds (not everyone will start poor)
                    buildGoal.CreateBuildGoal(StructureCategory.Home, StructureType.Small_House, EconomicClass.Poor);
                }
            } else if (shelterScore >= 40) { //&& nighttime=true
                if (!buildGoal.hasBuildGoal) {
                    // Build a campsite because it's night and we're 200+ tiles away from home.
                }
            } else if (shelterScore >= 80) {
                if (!buildGoal.hasBuildGoal) {
                    // Build a campsite far from home. Won't make it back before nightfall.
                }
            }
        }

        public abstract void JoinCommunity();

        public abstract void PickProfession();

        public virtual void ResetAgent() {

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

