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
                if (npcBrain.resourceTileTarget.occupiedType.Contains("_node_center")) {
                    if (npcBrain.useAdvAI) {
                        if (hasHarvestPos && npcBrain.pathAgent.isStopped && npcBrain.pathAgent.remainingDistance < 0.1) {
                            if (harvestableResource.GetHealth() > 0) {
                                if (animator.GetCurrentAnimatorStateInfo(0).IsTag("idle") && resourceAnimator.GetCurrentAnimatorStateInfo(0).IsTag("idle")) {
                                    HitResourceTarget();
                                }
                            } else if (animator.GetCurrentAnimatorStateInfo(0).IsTag("idle")) {
                                harvestableResource.RecycleAndSpawnLoot();
                                npcBrain.resourceTileTarget = null;
                                finished = true;
                            }
                        } else {
                            FindHarvestPosition();
                        }
                    } else {
                        // TODO: Create simple AI code for offscreen NPCs
                    }
                } else {
                    npcBrain.resourceTileTarget = null;
                    finished = true;
                }
            }
        }

        private void FindHarvestPosition() {
            // determine resource node animations depending on character position
            Vector3 resourceTileWorldPos = MapManager.Instance.GetWorldTileGrid().GetWorldPosition(npcBrain.resourceTileTarget.x, npcBrain.resourceTileTarget.y) + new Vector3(0.5f, 0.5f); // offset to get center of tile
            Vector3 closestHarvestPos = resourceTileWorldPos; 
            List<Vector3> harvestPosList = harvestableResource.resourceData.harvestSpots;
            
            for (int i = 0; i < harvestPosList.Count; i++) {
                Vector3 harvestPos = resourceTileWorldPos + harvestPosList[i];
                
                if (Vector3.Distance(npcBrain.transform.position, harvestPos) < Vector3.Distance(npcBrain.transform.position, closestHarvestPos)) {
                    closestHarvestPos = harvestPos;
                    closestIndex = i;
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
            if (harvestableResource.resourceData.resourceType.Equals(ResourceType.Wood)) {
                animator.Play("HarvestWood");
            }

            // hit resource to knock hitpoints off
            harvestableResource.ToolHit(5);
        }

        public void OnEnter() {
            //npcBrain.ResetAgent();
            finished = false;

            if (npcBrain.resourceTileTarget.occupiedType.Contains("_node_center")) {
                if (npcBrain.useAdvAI) {
                    resourceAnimator = npcBrain.resourceTileTarget.GetTileObject().GetComponentInChildren<Animator>();
                    //resourceAnimator.enabled = true;
                    harvestableResource = npcBrain.resourceTileTarget.GetTileObject().GetComponent<HarvestableResource>();
                }
                //DEBUG
                if (npcBrain.debugLogs) {
                    Debug.Log("HarvestResource.resourceTarget: " + npcBrain.resourceTileTarget.occupiedType);
                }
            } else {
                npcBrain.resourceTileTarget = null;
                finished = true;
            }
        }

        public void OnExit() {
            finished = false;
            hasHarvestPos = false;

            if (npcBrain.useAdvAI) {
                resourceAnimator = null;
                harvestableResource = null;
                //resourceAnimator.enabled = false;
            }
        }
    }
}

