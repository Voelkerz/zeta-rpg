using System.Collections;
using UnityEngine;

namespace ZetaGames.RPG {
    public class FellTreeTask : Task {

        public GameObject charGameObject;
        public GameObject treeGameObject;
        private Animator charAnimator;
        private Animator treeAnimator;
        private MoreMountains.TopDownEngine.Health treeHealth;

        public FellTreeTask() {
            initialised = false;
        }

        public override bool valid {
            get {
                if (charGameObject == null || treeGameObject == null || taskID == 0 || priority == -1) {
                    Debug.LogWarning("FellTreeTask - Task was not setup correctly!");
                    return false;
                } else if (!treeGameObject.GetComponent<Collider2D>().IsTouching(charGameObject.GetComponent<Collider2D>())) {
                    Debug.LogWarning("FellTreeTask - Player collider not overlapping tree collider!");
                    return false;
                } else {
                    return true;
                }
            }
        }

        public override void OnTaskStart() {
            Debug.Log(charGameObject.name + " starts to chop down " + treeGameObject.name);
        }

        public override void Initialise() {
            charAnimator = charGameObject.GetComponentInChildren<Animator>();
            treeAnimator = treeGameObject.GetComponentInChildren<Animator>();
            treeHealth = treeGameObject.GetComponent<MoreMountains.TopDownEngine.Health>();
            initialised = true;
        }

        //Execute() needs to be called in update of the TaskManager.
        public override void Execute() {
            // if tree is still alive
            if (treeHealth.CurrentHealth > 0) {
                // if character is not doing a chopping animation and tree is idle
                if (charAnimator.GetCurrentAnimatorStateInfo(0).IsTag("idling") && treeAnimator.GetCurrentAnimatorStateInfo(0).IsTag("idling")) {
                    
                    /************************************************
                     * TODO: I think I can generalize the animation
                     * names to work for all trees and races
                     ************************************************/

                    
                    // if character is a human
                    if (charGameObject.GetComponent<StatManager>().race.Equals(Race.Human)) {
                        charAnimator.Play("HumanBase_Logging");
                    }
                    
                    // if it is an oak tree
                    if (treeGameObject.name.Contains("Oak")) {
                        if (charGameObject.transform.position.x - treeGameObject.transform.position.x > 0) {
                            treeAnimator.Play("OakChopFromRight");
                        } else {
                            treeAnimator.Play("OakChopFromLeft");
                        }
                    }

                    treeHealth.SetHealth(treeHealth.CurrentHealth - 5);
                }
            } else {
                if (charAnimator.GetCurrentAnimatorStateInfo(0).IsTag("idling") && treeHealth.CurrentHealth <= 0) {
                    treeHealth.Kill();
                    //treeGameObject.GetComponent<MoreMountains.TopDownEngine.Loot>().SpawnLoot();
                    Debug.Log("Time: " + Time.time);
                    WaitToDestroy(2f);
                    _finished = true;
                }
            }
        }

        private IEnumerator WaitToDestroy(float waitTime) {
            yield return new WaitForSeconds(waitTime);
            GameObject.Destroy(treeGameObject);
            Debug.Log("Time Destroyed: " + Time.time);
        }

        public override bool Finished() {
            return _finished;
        }
    }
}
