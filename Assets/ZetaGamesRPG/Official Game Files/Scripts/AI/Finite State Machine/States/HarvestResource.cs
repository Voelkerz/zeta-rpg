using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    /*
     * 
     *  IDEA:
     *  - Add tool damage/durability to this script
     *   
    */

    public class HarvestResource : State {
        public override bool IsFinished { get => finished; }
        public override bool IsInterruptable { get => true; }
        private bool finished;
        private static readonly int lastDirectionX = Animator.StringToHash("AnimLastMoveX");
        private static readonly int lastDirectionY = Animator.StringToHash("AnimLastMoveY");
        private readonly AIBrain npcBrain;
        private readonly Animator animator;
        private ResourceNodeData resourceNodeData;
        private string memoryTag;
        //private bool depleted = false;
        private int currentHitPoints;
        private int closestIndex;
        private bool hasHarvestPos;
        private float harvestTimer;
        private Vector3 resourceTileWorldPos;
        private Vector3 northEast;
        private Vector3 northWest;
        private Vector3 southEast;
        private Vector3 southWest;

        public HarvestResource(AIBrain npcBrain) {
            this.npcBrain = npcBrain;
            animator = npcBrain.animator;
        }

        public override void Tick() {
            if (!finished) {
                if (hasHarvestPos && npcBrain.resourceTileTarget.lockTag == npcBrain.GetNpcLockTag() && npcBrain.pathMovement.isStopped && Vector3.Distance(npcBrain.transform.position, npcBrain.resourceTileTarget.GetWorldPosition() + MapManager.Instance.GetTileOffset()) <= 4f) {
                    //Debug.Log("HarvestResource.Tick(): Found harvest position and in place.");
                    if (currentHitPoints > 0) {
                        //Debug.Log("HarvestResource.Tick(): Current hitpoints above 0.");
                        if (harvestTimer >= 0f) {
                            //Debug.Log("HarvestResource.Tick(): Harvesting.");
                            harvestTimer = 0;
                            HitResourceTarget();
                        } else {
                            //Debug.Log("HarvestResource.Tick(): Waiting on harvest timer here.");
                            harvestTimer += npcBrain.deltaTime;
                        }
                    } else if (harvestTimer >= 0f) {
                        //Debug.Log("HarvestResource.Tick(): Finished harvesting.");
                        RecycleAndSpawnLoot();
                        npcBrain.resourceTileTarget.lockTag = -1;
                        npcBrain.resourceTileTarget = null;
                        finished = true;
                    } else {
                        //Debug.Log("HarvestResource.Tick(): Waiting on harvest timer.");
                        harvestTimer += npcBrain.deltaTime;
                    }
                } else if (!hasHarvestPos) {
                    //Debug.Log("HarvestResource.Tick(): Going to find a harvest position");
                    FindHarvestPosition();
                } else {
                    //Debug.Log("HarvestResource.Tick(): Moving closer to position");
                }
            } else {
                //Debug.LogWarning("HarvestResource.Tick(): State finished. Nothing happening.");
            }
        }

        private void FindHarvestPosition() {
            // determine resource node animations depending on character position
            List<Vector3> harvestPosList = resourceNodeData.harvestPositions;
            Vector3 closestHarvestPos = harvestPosList[0];

            for (int i = 0; i < harvestPosList.Count; i++) {
                Vector3 harvestPos = resourceTileWorldPos + harvestPosList[i];

                if (MapManager.Instance.GetWorldTileGrid().IsWithinGridBounds((int)harvestPos.x, (int)harvestPos.y)) {
                    if (MapManager.Instance.GetWorldTileGrid().GetGridObject(harvestPos) != null) {
                        if (MapManager.Instance.GetWorldTileGrid().GetGridObject(harvestPos).walkable) {
                            if (Vector3.Distance(npcBrain.transform.position, harvestPos) < Vector3.Distance(npcBrain.transform.position, closestHarvestPos)) {
                                closestHarvestPos = harvestPos;
                                closestIndex = i;
                            }
                        }
                    }
                }
            }
            //Debug.Log("HarvestResource.Tick(): Have harvest position.");
            npcBrain.pathMovement.destination = closestHarvestPos;
            npcBrain.pathMovement.SearchPath();
            hasHarvestPos = true;
        }

        private void HitResourceTarget() {
            if (npcBrain.useAdvAI) {
                //Debug.LogWarning("Using advanced AI!");

                // resource node animation
                switch (closestIndex) {
                    case 0:
                        npcBrain.pathMovement.SetPreviousDirection(northEast);
                        animator.SetFloat(lastDirectionX, 1);
                        animator.SetFloat(lastDirectionY, 1);

                        if (resourceNodeData.resourceCategory == ResourceCategory.Wood) {
                            npcBrain.PlayResourceSpriteAnimation(npcBrain.resourceTileTarget, 2, resourceNodeData.spriteAnimationList, 0);
                        }
                        break;
                    case 1:
                        npcBrain.pathMovement.SetPreviousDirection(southEast);
                        animator.SetFloat(lastDirectionX, 1);
                        animator.SetFloat(lastDirectionY, -1);

                        if (resourceNodeData.resourceCategory == ResourceCategory.Wood) {
                            npcBrain.PlayResourceSpriteAnimation(npcBrain.resourceTileTarget, 2, resourceNodeData.spriteAnimationList, 0);
                        }
                        break;
                    case 2:
                        npcBrain.pathMovement.SetPreviousDirection(southWest);
                        animator.SetFloat(lastDirectionX, -1);
                        animator.SetFloat(lastDirectionY, -1);

                        if (resourceNodeData.resourceCategory == ResourceCategory.Wood) {
                            npcBrain.PlayResourceSpriteAnimation(npcBrain.resourceTileTarget, 2, resourceNodeData.spriteAnimationList, 6);
                        }
                        break;
                    case 3:
                        npcBrain.pathMovement.SetPreviousDirection(northWest);
                        animator.SetFloat(lastDirectionX, -1);
                        animator.SetFloat(lastDirectionY, 1);

                        if (resourceNodeData.resourceCategory == ResourceCategory.Wood) {
                            npcBrain.PlayResourceSpriteAnimation(npcBrain.resourceTileTarget, 2, resourceNodeData.spriteAnimationList, 6);
                        }
                        break;
                    default:
                        break;
                }

                // determine which character animation to play
                if (resourceNodeData.resourceCategory == ResourceCategory.Wood) {
                    animator.Play("HarvestWood");
                } else if (resourceNodeData.resourceCategory == ResourceCategory.Stone || resourceNodeData.resourceCategory == ResourceCategory.Ore) {
                    animator.Play("Mining");
                }
            }

            // hit resource to knock hitpoints off
            ToolHit(5);
        }

        public bool ToolHit(int hitAmount) {
            //Debug.Log("Hit resource node");
            if (currentHitPoints > 0) {
                currentHitPoints -= hitAmount;
                return true;
            } else {
                return false;
            }
        }

        public void RecycleAndSpawnLoot() {
            int lootPerDrop = resourceNodeData.lootPerDrop;
            int currentTileLoot = npcBrain.resourceTileTarget.lootAvailable - lootPerDrop;
            memoryTag = resourceNodeData.resourceDropData.resourceType.ToString() + resourceNodeData.resourceDropData.resourceCategory.ToString() + ZetaUtilities.OCCUPIED_ITEMPICKUP;

            if (currentTileLoot <= 0) {
                // Spawn depleted node in current tile
                npcBrain.resourceTileTarget.occupiedStatus = ZetaUtilities.OCCUPIED_NODE_DEPLETED;
                npcBrain.resourceTileTarget.lockTag = -1;
                npcBrain.SetTilemapSpriteAsync(npcBrain.resourceTileTarget, 2, resourceNodeData.spriteDepleted);
                npcBrain.SetTilemapSpriteAsync(npcBrain.resourceTileTarget, 3, resourceNodeData.spriteDepletedShadow);
            } else {
                npcBrain.resourceTileTarget.lootAvailable -= lootPerDrop;
            }

            // Spawn loot in the adjacent tiles around node
            List<WorldTile> possibleLootPositions = new List<WorldTile>();

            // create list of possible locations for loot to spawn
            for (int x = 0; x < 4; x++) {
                for (int y = 0; y < 3; y++) {
                    // check grid bounds
                    if (MapManager.Instance.GetWorldTileGrid().IsWithinGridBounds((int)resourceTileWorldPos.x + (x - 1), (int)resourceTileWorldPos.y + (y - 1))) {
                        WorldTile adjTile = MapManager.Instance.GetWorldTileGrid().GetGridObject((int)resourceTileWorldPos.x + (x - 1), (int)resourceTileWorldPos.y + (y - 1));

                        // If adjacent tile is not occupied and is walkable
                        if (!adjTile.occupied && adjTile.walkable) {
                            possibleLootPositions.Add(adjTile);
                        }
                    }
                }
            }

            // Spawn max number of loot on random viable adjacent tiles
            for (int i = 0; i < lootPerDrop; i++) {
                WorldTile chosenTile = possibleLootPositions[Random.Range(0, possibleLootPositions.Count - 1)];
                possibleLootPositions.Remove(chosenTile);

                npcBrain.SetTilemapSpriteAsync(chosenTile, 4, resourceNodeData.resourceDropData.itemDropSprite);

                // Adjust tile data
                chosenTile.tileObjectData = resourceNodeData.resourceDropData;
                chosenTile.occupiedStatus = ZetaUtilities.OCCUPIED_ITEMPICKUP;
                chosenTile.occupiedCategory = resourceNodeData.resourceDropData.resourceCategory;
                chosenTile.occupiedType = resourceNodeData.resourceDropData.resourceType;
                chosenTile.occupied = true;

                // add resource drop to NPC memory so they'll go back for it later, if needed
                npcBrain.npcMemory.AddMemory(memoryTag + chosenTile.GetHashCode(), chosenTile.GetWorldPosition() + MapManager.Instance.GetTileOffset());
            }
        }

        public override void OnEnter() {
            finished = false;
            harvestTimer = 0;

            if (npcBrain.resourceTileTarget != null) {
                if (npcBrain.resourceTileTarget.tileObjectData != null && npcBrain.resourceTileTarget.occupiedStatus.Contains(ZetaUtilities.OCCUPIED_NODE_FULL) && npcBrain.resourceTileTarget.lockTag == npcBrain.GetNpcLockTag()) {
                    // Get the resource node data
                    if (typeof(ResourceNodeData).IsInstanceOfType(npcBrain.resourceTileTarget.tileObjectData)) {
                        resourceNodeData = (ResourceNodeData)npcBrain.resourceTileTarget.tileObjectData;
                    }

                    resourceTileWorldPos = npcBrain.resourceTileTarget.GetWorldPosition() + MapManager.Instance.GetTileOffset(); // offset to get center of tile
                    currentHitPoints = resourceNodeData.maxHitPoints;
                    northEast = new Vector3(1, 1);
                    southEast = new Vector3(1, -1);
                    northWest = new Vector3(-1, 1);
                    southWest = new Vector3(-1, -1);

                    //DEBUG
                    if (npcBrain.debugLogs) {
                        Debug.Log("HarvestResource.OnEnter(): Resource target tile: " + npcBrain.resourceTileTarget.occupiedCategory);
                    }
                } else {
                    Debug.LogWarning("HarvestResource.OnEnter(): ResourceTileTarget is not a harvestable node or already locked by other NPC.");
                    npcBrain.resourceTileTarget = null;
                    finished = true;
                }
            } else {
                finished = true;
            }
        }

        public override void OnExit() {
            if (!finished) {
                npcBrain.SetTilemapSpriteAsync(npcBrain.resourceTileTarget, 2, resourceNodeData.spriteFull);
                npcBrain.SetTilemapSpriteAsync(npcBrain.resourceTileTarget, 3, resourceNodeData.spriteFullShadow);
                npcBrain.resourceTileTarget.lockTag = -1;
            }

            npcBrain.resourceTileTarget = null;
            resourceNodeData = null;
            hasHarvestPos = false;
            memoryTag = null;
        }
    }
}

