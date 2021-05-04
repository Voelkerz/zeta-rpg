using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    /*
     * 
     *  IDEA:
     *  - Should I add all scripts and animations to the tree right before its harvested?
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
        private Animator resourceAnimator;
        private HarvestableResource harvestableResource;
        public bool depleted = false;
        private int closestIndex;
        private bool hasHarvestPos;

        public HarvestResource(AIBrain npcBrain) {
            this.npcBrain = npcBrain;
            animator = npcBrain.animator;
        }

        public void Tick() {
            if (!finished) {
                if (npcBrain.resourceTileTarget.occupiedStatus.Contains(ZetaUtilities.OCCUPIED_NODE_FULL)) {
                    if (npcBrain.useAdvAI) {
                        if (hasHarvestPos && npcBrain.pathAgent.isStopped && npcBrain.pathAgent.remainingDistance < 0.1) {
                            if (harvestableResource.GetHealth() > 0) {
                                if (animator.GetCurrentAnimatorStateInfo(0).IsTag("idle") && resourceAnimator.GetCurrentAnimatorStateInfo(0).IsTag("idle")) {
                                    //Debug.Log("HarvestResource.Tick(): Harvesting.");
                                    HitResourceTarget();
                                }
                            } else if (animator.GetCurrentAnimatorStateInfo(0).IsTag("idle")) {
                                //Debug.Log("HarvestResource.Tick(): Finished harvesting.");
                                harvestableResource.RecycleAndSpawnLoot();
                                npcBrain.resourceTileTarget = null;
                                finished = true;
                            }
                        } else if (!hasHarvestPos) {
                            //Debug.Log("HarvestResource.Tick(): Going to find a harvest position");
                            FindHarvestPosition();
                        } else if (hasHarvestPos && !MapManager.Instance.GetWorldTileGrid().GetGridObject(npcBrain.pathAgent.destination).walkable) {
                            // if harvest position destination is unwalkable (another creature could be standing in the spot), then nullify position
                            hasHarvestPos = false;
                        }
                    } else {
                        Debug.Log("HarvestResource.Tick(): Doing simple AI task");
                        // TODO: Create simple AI code for offscreen NPCs
                    }
                } else {
                    Debug.Log("HarvestResource.Tick(): Node not a full resource");
                    npcBrain.resourceTileTarget = null;
                    finished = true;
                }
            } else {
                Debug.Log("HarvestResource.Tick(): State finished. Nothing happening.");
            }
        }

        private void FindHarvestPosition() {
            // determine resource node animations depending on character position
            Vector3 resourceTileWorldPos = MapManager.Instance.GetWorldTileGrid().GetWorldPosition(npcBrain.resourceTileTarget.x, npcBrain.resourceTileTarget.y) + new Vector3(0.5f, 0.5f); // offset to get center of tile
            List<Vector3> harvestPosList = harvestableResource.resourceData.harvestSpots;
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

        public void HitResourceTarget() {
            // resource node animation
            switch (closestIndex) {
                case 0:
                    animator.SetFloat(lastDirectionX, 1);
                    animator.SetFloat(lastDirectionY, 1);
                    resourceAnimator.Play("HarvestLeft");
                    break;
                case 1:
                    animator.SetFloat(lastDirectionX, 1);
                    animator.SetFloat(lastDirectionY, -1);
                    resourceAnimator.Play("HarvestLeft");
                    break;
                case 2:
                    animator.SetFloat(lastDirectionX, -1);
                    animator.SetFloat(lastDirectionY, -1);
                    resourceAnimator.Play("HarvestRight");
                    break;
                case 3:
                    animator.SetFloat(lastDirectionX, -1);
                    animator.SetFloat(lastDirectionY, 1);
                    resourceAnimator.Play("HarvestRight");
                    break;
                default:
                    break;
            }

            // determine which character animation to play
            if (harvestableResource.resourceData.resourceCategory.Equals(ResourceCategory.Wood)) {
                animator.Play("HarvestWood");
            }

            // hit resource to knock hitpoints off
            harvestableResource.ToolHit(5);
        }

        public void OnEnter() {
            //npcBrain.ResetAgent();
            finished = false;

            if (npcBrain.resourceTileTarget.occupiedStatus.Contains(ZetaUtilities.OCCUPIED_NODE_FULL)) {
                if (npcBrain.useAdvAI) {
                    harvestableResource = npcBrain.resourceTileTarget.GetTileObject().GetComponent<HarvestableResource>();
                    resourceAnimator = npcBrain.resourceTileTarget.GetTileObject().GetComponentInChildren<Animator>();
                    npcBrain.resourceTileTarget.GetTileObject().tag = ZetaUtilities.TAG_CULLLOCKED;
                }
                //DEBUG
                if (npcBrain.debugLogs) {
                    Debug.Log("HarvestResource.resourceTarget: " + npcBrain.resourceTileTarget.occupiedStatus);
                }
            } else {
                Debug.LogWarning("HarvestResource.OnEnter(): ResourceTileTarget is not a full node.");
                npcBrain.resourceTileTarget = null;
                finished = true;
            }
        }

        public void OnExit() {
            if (npcBrain.useAdvAI) {
                resourceAnimator = null;
                harvestableResource = null;

                if (!finished) {
                    npcBrain.resourceTileTarget.GetTileObject().tag = ZetaUtilities.TAG_CULLED;
                }
            }

            finished = false;
            hasHarvestPos = false;
        }
    }
}

