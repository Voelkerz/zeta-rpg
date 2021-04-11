using UnityEngine;

namespace ZetaGames.RPG {
    internal class HarvestResource : IState {
        public bool isFinished { get => finished;}
        public bool isInterruptable { get => true;}
        private bool finished;
        private readonly AIBrain npcBrain;
        private readonly Animator animator;
        private Animator resourceAnimator;
        private ResourceType resourceType;
        private MoreMountains.TopDownEngine.Health treeHealth;
        public bool depleted = false;
        private GameObject resourceObject;
        private int currentHealth;
        

        public HarvestResource(AIBrain npcBrain, Animator animator, ResourceType resourceType) {
            this.npcBrain = npcBrain;
            this.animator = animator;
            this.resourceType = resourceType;
            
        }

        public void Tick() {
            if (!depleted) {
                if (npcBrain.resourceNodeTarget != null) {
                    if (currentHealth > 0) {
                        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("idle") && resourceAnimator.GetCurrentAnimatorStateInfo(0).IsTag("idle")) {
                            HitResourceTarget();
                        }
                    } else if (animator.GetCurrentAnimatorStateInfo(0).IsTag("idle")) {
                        depleted = true;
                        resourceObject.SetActive(false);

                        npcBrain.DestroyObject(resourceObject, 0.75f); // TODO: Need to implement a recycling system instead of destroy outright
                    }
                }
                /*
                if (npcBrain.resourceNodeTarget != null || treeHealth.gameObject.activeSelf) {
                    if (treeHealth != null && treeHealth.CurrentHealth > 0) {
                        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("idle") && resourceAnimator.GetCurrentAnimatorStateInfo(0).IsTag("idle")) {
                            HitResourceTarget();
                        }
                    } else if (treeHealth != null && animator.GetCurrentAnimatorStateInfo(0).IsTag("idle")) {
                        depleted = true;
                        treeHealth.Kill();
                        npcBrain.DestroyObject(treeHealth.gameObject, 0.75f); // TODO: Need to implement a recycling system instead of destroy outright
                    }
                }
                */
            } else {
                Debug.Log("resource node depleted...");
            }
        }

        public void HitResourceTarget() {
            if (resourceType.Equals(ResourceType.Wood)) {
                animator.Play("HarvestWood");
            }

            if (npcBrain.gameObject.transform.position.x - npcBrain.resourceNodeTarget.transform.position.x > 0) {
                resourceAnimator.Play("HarvestRight");
            } else {
                resourceAnimator.Play("HarvestLeft");
            }

            //treeHealth.SetHealth(treeHealth.CurrentHealth - 5);
            currentHealth -= 5;
        }

        public void OnEnter() {
            npcBrain.ResetAgent();
            resourceAnimator = npcBrain.resourceNodeTarget.GetComponentInParent<Animator>();
            //treeHealth = npcBrain.resourceNodeTarget.GetComponentInParent<MoreMountains.TopDownEngine.Health>();
            resourceObject = npcBrain.resourceNodeTarget.transform.parent.transform.parent.gameObject;
            depleted = false;
            currentHealth = 20;
        }

        public void OnExit() {
            depleted = false;
        }
    }
}

