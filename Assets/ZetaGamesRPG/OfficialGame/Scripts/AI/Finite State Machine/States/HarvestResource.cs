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
        public override int priority => 10;
        public override bool isFinished { get => finished; }
        public override bool isInterruptable { get => true; }

        private bool finished;
        //private static readonly int lastDirectionX = Animator.StringToHash("AnimLastMoveX");
        //private static readonly int lastDirectionY = Animator.StringToHash("AnimLastMoveY");
        private readonly AIBrain npc;
        private ResourceNode resourceNode;
        public WorldTile harvestTarget;
        public bool hasHarvestTarget;
        private string memoryTag;
        private int currentHitPoints;
        private int closestIndex;
        private bool hasHarvestPos;
        //private float harvestTimer;
        private Vector3 harvestTargetWorldPos;
        private Vector3 northEast;
        private Vector3 northWest;
        private Vector3 southEast;
        private Vector3 southWest;

        public HarvestResource(AIBrain npc) {
            this.npc = npc;
        }

        public override void Tick() {
            if (!finished) {
                if (hasHarvestPos && harvestTarget.lockTag == npc.lockTag && npc.pathMovement.isStopped && Vector3.Distance(npc.transform.position, harvestTarget.GetWorldPosition() + MapManager.Instance.GetTileOffset()) <= 4f) {
                    //Debug.Log("HarvestResource.Tick(): Have harvest position and in place.");
                    if (currentHitPoints > 0) {
                        // Hit resource node with correct tool
                        HitResourceTarget();
                    } else {
                        //Debug.Log("HarvestResource.Tick(): Finished harvesting.");
                        RecycleAndSpawnLoot();
                        harvestTarget.lockTag = -1;
                        hasHarvestTarget = false;
                        finished = true;
                    }
                } else if (!hasHarvestPos) {
                    //Debug.Log("HarvestResource.Tick(): Going to find a harvest position");
                    FindHarvestPosition();
                } else if (harvestTarget.lockTag != npc.lockTag) {
                    Debug.LogWarning("HarvestResource.Tick(): Harvest tile target locked by another NPC.");
                    hasHarvestTarget = false;
                    finished = true;
                } else {
                    //Debug.Log("HarvestResource.Tick(): Moving closer to position");
                }
            } else {
                Debug.LogWarning("HarvestResource.Tick(): State finished. Nothing happening.");
            }
        }

        private void FindHarvestPosition() {
            // determine resource node animations depending on character position
            List<Vector3> harvestPosList = resourceNode.harvestPositions;
            Vector3 closestHarvestPos = harvestPosList[0];

            for (int i = 0; i < harvestPosList.Count; i++) {
                Vector3 harvestPos = harvestTargetWorldPos + harvestPosList[i];

                if (MapManager.Instance.GetWorldTileGrid().IsWithinGridBounds((int)harvestPos.x, (int)harvestPos.y)) {
                    if (MapManager.Instance.GetWorldTileGrid().GetGridObject(harvestPos) != null) {
                        if (MapManager.Instance.GetWorldTileGrid().GetGridObject(harvestPos).walkable) {
                            if (Vector3.Distance(npc.transform.position, harvestPos) < Vector3.Distance(npc.transform.position, closestHarvestPos)) {
                                closestHarvestPos = harvestPos;
                                closestIndex = i;
                            }
                        }
                    }
                }
            }
            //Debug.Log("HarvestResource.Tick(): Have harvest position.");
            npc.pathMovement.destination = closestHarvestPos;
            npc.pathMovement.SearchPath();
            hasHarvestPos = true;
        }

        private void HitResourceTarget() {
            if (npc.useAdvAI) {
                // resource node animation
                switch (closestIndex) {
                    case 0:
                        npc.pathMovement.SetPreviousDirection(northEast);
                        npc.animationController.animMoveX = 1;
                        npc.animationController.animMoveY = 1;
                        //animator.SetFloat(lastDirectionX, 1);
                        //animator.SetFloat(lastDirectionY, 1);

                        if (resourceNode.resourceCategory == ResourceCategory.Wood) {
                            npc.PlayResourceSpriteAnimation(harvestTarget, 2, resourceNode.spriteAnimationList, 0);
                        }
                        break;
                    case 1:
                        npc.pathMovement.SetPreviousDirection(southEast);
                        npc.animationController.animMoveX = 1;
                        npc.animationController.animMoveY = -1;
                        //animator.SetFloat(lastDirectionX, 1);
                        //animator.SetFloat(lastDirectionY, -1);

                        if (resourceNode.resourceCategory == ResourceCategory.Wood) {
                            npc.PlayResourceSpriteAnimation(harvestTarget, 2, resourceNode.spriteAnimationList, 0);
                        }
                        break;
                    case 2:
                        npc.pathMovement.SetPreviousDirection(southWest);
                        npc.animationController.animMoveX = -1;
                        npc.animationController.animMoveY = -1;
                        //animator.SetFloat(lastDirectionX, -1);
                        //animator.SetFloat(lastDirectionY, -1);

                        if (resourceNode.resourceCategory == ResourceCategory.Wood) {
                            npc.PlayResourceSpriteAnimation(harvestTarget, 2, resourceNode.spriteAnimationList, 6);
                        }
                        break;
                    case 3:
                        npc.pathMovement.SetPreviousDirection(northWest);
                        npc.animationController.animMoveX = -1;
                        npc.animationController.animMoveY = 1;
                        //animator.SetFloat(lastDirectionX, -1);
                        //animator.SetFloat(lastDirectionY, 1);

                        if (resourceNode.resourceCategory == ResourceCategory.Wood) {
                            npc.PlayResourceSpriteAnimation(harvestTarget, 2, resourceNode.spriteAnimationList, 6);
                        }
                        break;
                    default:
                        break;
                }

                // determine which character animation to play
                if (resourceNode.resourceCategory == ResourceCategory.Wood) {
                    npc.animationController.PlayAnimation(AnimationType.Logging);
                    //animator.Play("HarvestWood");
                } else if (resourceNode.resourceCategory == ResourceCategory.Stone || resourceNode.resourceCategory == ResourceCategory.Ore) {
                    npc.animationController.PlayAnimation(AnimationType.Mining);
                    //animator.Play("Mining");
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
            int lootPerDrop = resourceNode.lootPerDrop;
            int currentTileLoot = harvestTarget.lootAvailable - lootPerDrop;
            memoryTag = resourceNode.resourceItemData.name + ZetaUtilities.OCCUPIED_ITEMPICKUP;

            if (currentTileLoot <= 0) {
                // Spawn depleted node in current tile
                harvestTarget.occupiedStatus = ZetaUtilities.OCCUPIED_NODE_DEPLETED;
                harvestTarget.lockTag = -1;
                npc.SetTilemapSpriteAsync(harvestTarget, 2, resourceNode.spriteDepleted);
                npc.SetTilemapSpriteAsync(harvestTarget, 3, resourceNode.spriteDepletedShadow);
            } else {
                harvestTarget.lootAvailable -= lootPerDrop;
            }

            // Spawn loot in the adjacent tiles around node
            List<WorldTile> possibleLootPositions = new List<WorldTile>();

            // create list of possible locations for loot to spawn
            for (int x = 0; x < 4; x++) {
                for (int y = 0; y < 3; y++) {
                    // check grid bounds
                    if (MapManager.Instance.GetWorldTileGrid().IsWithinGridBounds((int)harvestTargetWorldPos.x + (x - 1), (int)harvestTargetWorldPos.y + (y - 1))) {
                        WorldTile adjTile = MapManager.Instance.GetWorldTileGrid().GetGridObject((int)harvestTargetWorldPos.x + (x - 1), (int)harvestTargetWorldPos.y + (y - 1));

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

                npc.SetTilemapSpriteAsync(chosenTile, 4, resourceNode.resourceItemData.itemDropSprite);

                // Adjust tile data
                chosenTile.tileObject = resourceNode.resourceItemData;
                chosenTile.occupiedStatus = ZetaUtilities.OCCUPIED_ITEMPICKUP;
                chosenTile.occupiedCategory = resourceNode.resourceItemData.resourceCategory;
                chosenTile.occupiedType = resourceNode.resourceItemData.resourceType;
                chosenTile.occupied = true;

                // Add resource drop to NPC memory so they'll go back for it later, if needed
                npc.memory.AddMemory(memoryTag + chosenTile.GetHashCode(), chosenTile.GetWorldPosition() + MapManager.Instance.GetTileOffset());
            }
        }

        public override void OnEnter() {
            finished = false;
            hasHarvestPos = false;

            if (hasHarvestTarget) {
                if (harvestTarget.tileObject != null && harvestTarget.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_NODE_FULL)) {
                    if (harvestTarget.lockTag == npc.lockTag) {
                        // Get the resource node data
                        if (typeof(ResourceNode).IsInstanceOfType(harvestTarget.tileObject)) {
                            resourceNode = (ResourceNode)harvestTarget.tileObject;

                            harvestTargetWorldPos = harvestTarget.GetWorldPosition() + MapManager.Instance.GetTileOffset(); // offset to get center of tile
                            currentHitPoints = resourceNode.maxHitPoints;
                            northEast = new Vector3(1, 1);
                            southEast = new Vector3(1, -1);
                            northWest = new Vector3(-1, 1);
                            southWest = new Vector3(-1, -1);

                            //DEBUG
                            if (npc.debugLogs) {
                                Debug.Log("HarvestResource.OnEnter(): Harvest target: " + harvestTarget.occupiedCategory + " node");
                            }
                        } else {
                            Debug.LogError("HarvestResource.OnEnter(): Harvest tile target does not contain proper resource node data.");
                            hasHarvestTarget = false;
                            finished = true;
                        }
                    } else {
                        Debug.LogWarning("HarvestResource.OnEnter(): Harvest tile is locked by another NPC.");
                        hasHarvestTarget = false;
                        finished = true;
                    }
                } else {
                    Debug.LogWarning("HarvestResource.OnEnter(): ResourceTileTarget is not a harvestable node.");
                    hasHarvestTarget = false;
                    finished = true;
                }
            } else {
                finished = true;
            }
        }

        public override void OnExit() {
            if (!finished) {
                npc.SetTilemapSpriteAsync(harvestTarget, 2, resourceNode.spriteFull);
                npc.SetTilemapSpriteAsync(harvestTarget, 3, resourceNode.spriteFullShadow);
                harvestTarget.lockTag = -1;
            }

            resourceNode = null;
            memoryTag = null;
            harvestTarget = null;
            hasHarvestPos = false;
        }
    }
}

