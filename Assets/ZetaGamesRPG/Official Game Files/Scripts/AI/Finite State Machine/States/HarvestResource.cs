using UnityEngine;

namespace ZetaGames.RPG {
    internal class HarvestResource : IState {
        public bool isFinished { get => finished;}
        public bool isInterruptable { get => true;}
        private bool finished;
        private static readonly int lastDirection = Animator.StringToHash("lastDirection");
        private readonly AIBrain npcBrain;
        private readonly Animator animator;
        private Animator resourceAnimator;
        private HarvestableResource harvestableResource;
        private ResourceType resourceType;
        public bool depleted = false;
        

        public HarvestResource(AIBrain npcBrain) {
            this.npcBrain = npcBrain;
            animator = npcBrain.animator;
            resourceType = npcBrain.resourceNeeded;
        }

        public void Tick() {
            if (!depleted) {
                if (npcBrain.resourceTarget != null) {
                    if (harvestableResource.GetHealth() > 0) {
                        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("idle") && resourceAnimator.GetCurrentAnimatorStateInfo(0).IsTag("idle")) {
                            HitResourceTarget();
                        }
                    } else if (animator.GetCurrentAnimatorStateInfo(0).IsTag("idle")) {
                        depleted = true;
                        harvestableResource.RecycleAndSpawnLoot();
                        npcBrain.resourceTarget = null;
                    }
                }
            } 
        }

        public void HitResourceTarget() {
            // hit resource to knock hitpoints off
            harvestableResource.ToolHit(5);

            // Tree specific animations
            if (harvestableResource.GetResourceType().Equals(ResourceType.Wood)) {
                animator.Play("HarvestWood");

                if (npcBrain.gameObject.transform.position.x - npcBrain.resourceTarget.transform.position.x > 0 &&
                npcBrain.gameObject.transform.position.y - npcBrain.resourceTarget.transform.position.y < 0.5) {
                    // facing NW
                    resourceAnimator.Play("HarvestRight");
                    animator.SetFloat(lastDirection, 2f);
                } else if (npcBrain.gameObject.transform.position.x - npcBrain.resourceTarget.transform.position.x > 0 &&
                    npcBrain.gameObject.transform.position.y - npcBrain.resourceTarget.transform.position.y > 0.5) {
                    // facing SW
                    resourceAnimator.Play("HarvestRight");
                    animator.SetFloat(lastDirection, 1f);
                } else if (npcBrain.gameObject.transform.position.x - npcBrain.resourceTarget.transform.position.x < 0 &&
                            npcBrain.gameObject.transform.position.y - npcBrain.resourceTarget.transform.position.y < 0.5) {
                    // facing NE
                    resourceAnimator.Play("HarvestLeft");
                    animator.SetFloat(lastDirection, 0f);
                } else if (npcBrain.gameObject.transform.position.x - npcBrain.resourceTarget.transform.position.x < 0 &&
                            npcBrain.gameObject.transform.position.y - npcBrain.resourceTarget.transform.position.y > 0.5) {
                    // facing SE
                    resourceAnimator.Play("HarvestLeft");
                    animator.SetFloat(lastDirection, 3f);
                }
            }
        }

        public void OnEnter() {
            npcBrain.ResetAgent();

            if (npcBrain.resourceTarget != null) {
                if (npcBrain.debugLogs) {
                    Debug.Log("resourceTarget: " + npcBrain.resourceTarget.name);
                }
                resourceAnimator = npcBrain.resourceTarget.GetComponentInChildren<Animator>();
                harvestableResource = npcBrain.resourceTarget.GetComponent<HarvestableResource>();
            }

            depleted = false;
        }

        public void OnExit() {
            depleted = false;
        }
    }
}

