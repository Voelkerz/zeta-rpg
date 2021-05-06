using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ZetaGames.RPG {
    /*
     * 
     *  IDEA:
     *  - Add tool damage/durability to this script
     *   
    */

    public class HarvestResource : IState {
        public bool isFinished { get => finished; }
        public bool isInterruptable { get => true; }
        private bool finished;
        private static readonly int lastDirectionX = Animator.StringToHash("AnimLastMoveX");
        private static readonly int lastDirectionY = Animator.StringToHash("AnimLastMoveY");
        private readonly AIBrain npcBrain;
        private readonly Animator animator;
        public bool depleted = false;
        private int currentHitPoints;
        private int closestIndex;
        private bool hasHarvestPos;
        Vector3 resourceTileWorldPos;
        Vector3Int resourceTileGridPos;
        private float harvestTimer;

        public HarvestResource(AIBrain npcBrain) {
            this.npcBrain = npcBrain;
            animator = npcBrain.animator;
        }

        public void Tick() {
            if (!finished) {
                if (npcBrain.resourceTileTarget.occupiedStatus.Contains(ZetaUtilities.OCCUPIED_NODE_FULL)) {
                    if (hasHarvestPos && npcBrain.pathAgent.isStopped && npcBrain.pathAgent.remainingDistance < 0.1) {
                        if (currentHitPoints > 0) {
                            if (animator.GetCurrentAnimatorStateInfo(0).IsTag("idle") && harvestTimer > 1f) {
                                //Debug.Log("HarvestResource.Tick(): Harvesting.");
                                harvestTimer = 0;
                                HitResourceTarget();
                            } else {
                                harvestTimer += Time.deltaTime;
                            }
                        } else if (animator.GetCurrentAnimatorStateInfo(0).IsTag("idle") && harvestTimer > 1f) {
                            //Debug.Log("HarvestResource.Tick(): Finished harvesting.");
                            RecycleAndSpawnLoot();
                            npcBrain.resourceTileTarget = null;
                            finished = true;
                        } else {
                            harvestTimer += Time.deltaTime;
                        }
                    } else if (!hasHarvestPos) {
                        //Debug.Log("HarvestResource.Tick(): Going to find a harvest position");
                        FindHarvestPosition();
                    } else if (hasHarvestPos && !MapManager.Instance.GetWorldTileGrid().GetGridObject(npcBrain.pathAgent.destination).walkable) {
                        // if harvest position destination is unwalkable (another creature could be standing in the spot), then nullify position
                        hasHarvestPos = false;
                    }
                } else {
                    Debug.Log("HarvestResource.Tick(): Node not a full resource");
                    npcBrain.resourceTileTarget = null;
                    finished = true;
                }
            } else {
                Debug.LogWarning("HarvestResource.Tick(): State finished. Nothing happening.");
            }
        }

        private void FindHarvestPosition() {
            // determine resource node animations depending on character position
            List<Vector3> harvestPosList = npcBrain.resourceTileTarget.resourceNodeData.harvestPositions;
            Vector3 closestHarvestPos = harvestPosList[0];

            for (int i = 0; i < harvestPosList.Count; i++) {
                Vector3 harvestPos = resourceTileWorldPos + harvestPosList[i];

                if (MapManager.Instance.GetWorldTileGrid().GetGridObject(harvestPos).walkable) {
                    if (Vector3.Distance(npcBrain.transform.position, harvestPos) < Vector3.Distance(npcBrain.transform.position, closestHarvestPos)) {
                        closestHarvestPos = harvestPos;
                        closestIndex = i;
                    }
                }
            }

            npcBrain.pathAgent.destination = closestHarvestPos;
            npcBrain.pathAgent.SearchPath();
            hasHarvestPos = true;
        }

        private void HitResourceTarget() {
            // resource node animation
            switch (closestIndex) {
                case 0:
                    npcBrain.pathAgent.SetPreviousDirection(new Vector3(1, 1));
                    animator.SetFloat(lastDirectionX, 1);
                    animator.SetFloat(lastDirectionY, 1);
                    npcBrain.PlayResourceSpriteAnimation(npcBrain.resourceTileTarget, 2, npcBrain.resourceTileTarget.resourceNodeData.spriteAnimationList, 0);
                    break;
                case 1:
                    npcBrain.pathAgent.SetPreviousDirection(new Vector3(1, -1));
                    animator.SetFloat(lastDirectionX, 1);
                    animator.SetFloat(lastDirectionY, -1);
                    npcBrain.PlayResourceSpriteAnimation(npcBrain.resourceTileTarget, 2, npcBrain.resourceTileTarget.resourceNodeData.spriteAnimationList, 0);
                    break;
                case 2:
                    npcBrain.pathAgent.SetPreviousDirection(new Vector3(-1, -1));
                    animator.SetFloat(lastDirectionX, -1);
                    animator.SetFloat(lastDirectionY, -1);
                    npcBrain.PlayResourceSpriteAnimation(npcBrain.resourceTileTarget, 2, npcBrain.resourceTileTarget.resourceNodeData.spriteAnimationList, 6);
                    break;
                case 3:
                    npcBrain.pathAgent.SetPreviousDirection(new Vector3(-1, 1));
                    animator.SetFloat(lastDirectionX, -1);
                    animator.SetFloat(lastDirectionY, 1);
                    npcBrain.PlayResourceSpriteAnimation(npcBrain.resourceTileTarget, 2, npcBrain.resourceTileTarget.resourceNodeData.spriteAnimationList, 6);
                    break;
                default:
                    break;
            }

            // determine which character animation to play
            if (npcBrain.resourceTileTarget.resourceNodeData.resourceCategory.Equals(ResourceCategory.Wood)) {
                animator.Play("HarvestWood");
            }

            // hit resource to knock hitpoints off
            ToolHit(5);
        }

        public bool ToolHit(int hitAmount) {
            if (currentHitPoints > 0) {
                currentHitPoints -= hitAmount;
                return true;
            } else {
                return false;
            }
        }

        public void RecycleAndSpawnLoot() {
            // Spawn depleted node in current tile
            npcBrain.resourceTileTarget.occupiedStatus = ZetaUtilities.OCCUPIED_NODE_DEPLETED;
            npcBrain.SetTilemapSpriteAsync(npcBrain.resourceTileTarget, 2, npcBrain.resourceTileTarget.resourceNodeData.spriteDepleted);
            npcBrain.SetTilemapSpriteAsync(npcBrain.resourceTileTarget, 3, npcBrain.resourceTileTarget.resourceNodeData.spriteDepletedShadow);

            // Spawn loot in the adjacent tiles around node
            int numLoot = npcBrain.resourceTileTarget.resourceNodeData.lootPerDrop;
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
            for (int i = 0; i < numLoot; i++) {
                WorldTile chosenTile = possibleLootPositions[Random.Range(0, possibleLootPositions.Count - 1)];
                possibleLootPositions.Remove(chosenTile);
                chosenTile.SetTileObject(npcBrain.InstantiateObject(npcBrain.resourceTileTarget.resourceNodeData.lootPrefab, MapManager.Instance.GetWorldTileGrid().GetWorldPosition(chosenTile.x, chosenTile.y)));

                // Adjust tile data
                chosenTile.occupiedCategory = npcBrain.resourceTileTarget.resourceNodeData.resourceCategory.ToString();
                chosenTile.occupiedType = npcBrain.resourceTileTarget.resourceNodeData.resourceType.ToString();
                chosenTile.occupiedStatus = ZetaUtilities.OCCUPIED_ITEMPICKUP;
                chosenTile.occupied = true;
            }
        }

        public void OnEnter() {
            finished = false;
            harvestTimer = 0;

            if (npcBrain.resourceTileTarget.occupiedStatus.Contains(ZetaUtilities.OCCUPIED_NODE_FULL)) {
                resourceTileWorldPos = MapManager.Instance.GetWorldTileGrid().GetWorldPosition(npcBrain.resourceTileTarget.x, npcBrain.resourceTileTarget.y) + new Vector3(0.5f, 0.5f); // offset to get center of tile
                resourceTileGridPos = new Vector3Int(npcBrain.resourceTileTarget.x, npcBrain.resourceTileTarget.y, 0);
                currentHitPoints = npcBrain.resourceTileTarget.resourceNodeData.maxHitPoints;

                //DEBUG
                if (npcBrain.debugLogs) {
                    Debug.Log("HarvestResource.resourceTarget: " + npcBrain.resourceTileTarget.occupiedStatus);
                }
            } else {
                Debug.LogWarning("HarvestResource.OnEnter(): ResourceTileTarget is not a harvestable node.");
                npcBrain.resourceTileTarget = null;
                finished = true;
            }
        }

        public void OnExit() {
            if (!finished) {
                npcBrain.SetTilemapSpriteAsync(npcBrain.resourceTileTarget, 2, npcBrain.resourceTileTarget.resourceNodeData.spriteFull);
                npcBrain.SetTilemapSpriteAsync(npcBrain.resourceTileTarget, 3, npcBrain.resourceTileTarget.resourceNodeData.spriteFullShadow);
                npcBrain.resourceTileTarget = null;
            }

            npcBrain.pathAgent.canMove = true;
            finished = false;
            hasHarvestPos = false;
        }
    }
}

