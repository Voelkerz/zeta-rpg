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
        //private AudioManager resourceSound;
        private ResourceType resourceType;
        public bool depleted = false;
        private int currentHealth;
        

        public HarvestResource(AIBrain npcBrain) {
            this.npcBrain = npcBrain;
            animator = npcBrain.animator;
            resourceType = npcBrain.resourceNeeded;
        }

        public void Tick() {
            if (!depleted) {
                if (npcBrain.resourceTarget != null) {
                    if (currentHealth > 0) {
                        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("idle") && resourceAnimator.GetCurrentAnimatorStateInfo(0).IsTag("idle")) {
                            HitResourceTarget();
                        }
                    } else if (animator.GetCurrentAnimatorStateInfo(0).IsTag("idle")) {
                        depleted = true;
                        npcBrain.resourceTarget.SetActive(false);

                        npcBrain.DestroyObject(npcBrain.resourceTarget, 0.75f); // TODO: Need to implement a recycling system instead of destroy outright
                    }
                }
            } else {
                Debug.Log("resource node depleted...");
            }
        }

        public void HitResourceTarget() {
            if (resourceType.Equals(ResourceType.Wood)) {
                animator.Play("HarvestWood");


                if (npcBrain.gameObject.transform.position.x - npcBrain.resourceTarget.transform.position.x > 0 &&
                npcBrain.gameObject.transform.position.y - npcBrain.resourceTarget.transform.position.y < -1) {
                    resourceAnimator.Play("HarvestRight");
                    animator.SetFloat(lastDirection, 2f);
                } else if (npcBrain.gameObject.transform.position.x - npcBrain.resourceTarget.transform.position.x > 0 &&
                    npcBrain.gameObject.transform.position.y - npcBrain.resourceTarget.transform.position.y > -1) {
                    resourceAnimator.Play("HarvestRight");
                    animator.SetFloat(lastDirection, 1f);
                } else if (npcBrain.gameObject.transform.position.x - npcBrain.resourceTarget.transform.position.x < 0 &&
                            npcBrain.gameObject.transform.position.y - npcBrain.resourceTarget.transform.position.y < -1) {
                    resourceAnimator.Play("HarvestLeft");
                    animator.SetFloat(lastDirection, 0f);
                } else if (npcBrain.gameObject.transform.position.x - npcBrain.resourceTarget.transform.position.x < 0 &&
                            npcBrain.gameObject.transform.position.y - npcBrain.resourceTarget.transform.position.y > -1) {
                    resourceAnimator.Play("HarvestLeft");
                    animator.SetFloat(lastDirection, 3f);
                }
            }

            //resourceSound.PlaySoundEffect();

            //treeHealth.SetHealth(treeHealth.CurrentHealth - 5);
            currentHealth -= 5;
        }

        public void OnEnter() {
            npcBrain.ResetAgent();

            if (npcBrain.resourceTarget != null) {
                if (npcBrain.debugLogs) {
                    Debug.Log("resourceTarget: " + npcBrain.resourceTarget.name);
                }
                resourceAnimator = npcBrain.resourceTarget.GetComponentInChildren<Animator>();
                //resourceSound = npcBrain.resourceTarget.GetComponent<AudioManager>();
            }

            depleted = false;
            currentHealth = 20;
        }

        public void OnExit() {
            depleted = false;
        }
    }
}

