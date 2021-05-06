using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ZetaGames.RPG {
    public abstract class AIBrain : MonoBehaviour {
        protected StateMachineMultiCondition stateMachine;
        [HideInInspector] public Personality personality { get; set; }
        [HideInInspector] public AIMovement pathAgent { get; set; }
        [HideInInspector] public Animator animator { get; set; }
        [HideInInspector] public WorldTile resourceTileTarget { get; set; }
        [HideInInspector] public NpcMemory npcMemory { get; set; }
        [HideInInspector] public NpcInventory npcInventory { get; set; }
        [HideInInspector] public ResourceCategory resourceCategoryWanted { get; set; }
        [HideInInspector] public ResourceType resourceTypeWanted { get; set; }
        [HideInInspector] public bool useAdvAI { get; set; }
        private float tickTimer;

        // EDITOR PROPERTIES
        public bool inCombat;
        public bool debugLogs;

        private void Awake() {
            // Cache NPC components
            pathAgent = GetComponent<AIMovement>();
            animator = GetComponentInChildren<Animator>();
            npcInventory = GetComponent<NpcInventory>();

            // Create state machine for NPC
            stateMachine = new StateMachineMultiCondition();
            stateMachine.MonoParser(this);

            // Create a memory for npc
            npcMemory = new NpcMemory();

            // Set update timer to zero
            tickTimer = 0;
        }

        private IEnumerator Start() {
            useAdvAI = true;
            // Provide a random wakeup for NPCs
            yield return new WaitForSeconds(Random.Range(3, 6));
        }

        protected virtual void Update() {
            stateMachine.Tick();

            if (tickTimer > 0.1f) {
                tickTimer = 0;

                updateCooldownTimers();
                updateNeeds();
            } else {
                tickTimer = Time.deltaTime;
            }
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
    }
}

