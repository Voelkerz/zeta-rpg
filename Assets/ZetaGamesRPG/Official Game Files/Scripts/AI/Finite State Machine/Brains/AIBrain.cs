using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System.Collections;

namespace ZetaGames.RPG {
    public abstract class AIBrain : MonoBehaviour {
        protected StateMachineMultiCondition stateMachine;
        [HideInInspector] public Personality personality { get; set; }
        [HideInInspector] public AIMovement pathMovement { get; set; }
        [HideInInspector] public Seeker pathSeeker { get; set; }
        [HideInInspector] public Animator animator { get; set; }
        [HideInInspector] public WorldTile resourceTileTarget { get; set; }
        [HideInInspector] public NpcMemory npcMemory { get; set; }
        [HideInInspector] public NpcInventory npcInventory { get; set; }
        [HideInInspector] public ResourceCategory resourceCategoryWanted { get; set; }
        [HideInInspector] public ResourceType resourceTypeWanted { get; set; }
        [HideInInspector] public bool useAdvAI { get; set; }
        [HideInInspector] public float deltaTime { get; set; }

        [HideInInspector] public Dictionary<ResourceCategory, int> numMaterialsRequiredList = new Dictionary<ResourceCategory, int>();
        [HideInInspector] public Dictionary<ResourceCategory, ResourceType> specificMaterialTypeList = new Dictionary<ResourceCategory, ResourceType>();

        private WaitForSeconds[] waitTimers = new WaitForSeconds[3];
        private int npcLockTag;

        // EDITOR PROPERTIES
        public bool inCombat;
        public bool debugLogs;

        private void Awake() {
            // Cache NPC components
            pathMovement = GetComponent<AIMovement>();
            pathSeeker = GetComponent<Seeker>();
            animator = GetComponentInChildren<Animator>();
            npcInventory = GetComponent<NpcInventory>();

            // Create state machine for NPC
            stateMachine = new StateMachineMultiCondition();
            stateMachine.MonoParser(this);

            if (debugLogs) {
                stateMachine.debugLog = true;
            }

            // Create a memory for npc
            npcMemory = new NpcMemory();

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
                updateNeeds();

                deltaTime = 0;
            }

            deltaTime += Time.deltaTime;
        }

        public virtual int GetNpcLockTag() {
            return npcLockTag;
        }

        public virtual void ResetAgent() {

        }

        protected virtual void updateNeeds() {
            // Food
            // Energy (sleep)
        }

        protected virtual void updateCooldownTimers() {
            //wanderCooldown += Time.deltaTime;
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
            npcInventory.DropResource();
        }
    }
}

