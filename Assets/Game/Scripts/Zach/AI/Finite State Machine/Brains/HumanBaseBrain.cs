using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/***********************************************
 * TODO:
 * -Implement some kind of memory system (a List of Vector3's maybe) for key things
 *      like home, towns, stores, last location of a spotted enemy, a dangerous area,
 *      and simple things as the last place a resource node was found so it may aid
 *      in finding more.
 * -Implement a priority system for transitions in case there are multiple transitions
 *      that come back as true and possible.
************************************************/

namespace ZetaGames.RPG {
    public class HumanBaseBrain : AIBrain {
        

        private int maxCarried = 5;
        private int gathered = 0;
        
        private ResourceType resourceType = ResourceType.Wood;
        public float resourceSearchRange = 20f;

        private void Start() {
            // Cache NPC components
            //var enemyDetector = gameObject.AddComponent<EnemyDetector>();
            //var fleeParticleSystem = gameObject.GetComponentInChildren<ParticleSystem>();

            // Create all NPC states
            var searchForResourceNode = new SearchForResourceNode(this, resourceType, resourceSearchRange, 1 << 7);
            var searchForResourceDrop = new SearchForResourceDrop(this, resourceType, resourceSearchRange, 1 << 6);
            var moveToResource = new MoveToResource(this);
            var harvestResource = new HarvestResource(this, animator, resourceType);
            var moveToStockpile = new MoveToStockpile(this);
            var placeItemsInStockpile = new PlaceResourcesInStockpile(this);
            var returnHome = new GetUnstuck(this);
            var wander = new Wander(this, 10f, 10f); // range and cycle time
            //var flee = new Flee(this, navMeshAgent, enemyDetector, animator, fleeParticleSystem);

            /***************************************************************
             * SPECIFIC STATE TRANSITIONS
            ***************************************************************/
            // FROM 'search for resource drop' to ...
            AT(searchForResourceDrop, searchForResourceNode, new List<Func<bool>> { IsFalse(HasResourceDropTarget()), SearchAttempted() });
            AT(searchForResourceDrop, moveToResource, new List<Func<bool>> { HasResourceDropTarget(), IsFalse(InventoryFull()) });

            // FROM 'search for resource node' to ...
            AT(searchForResourceNode, moveToResource, new List<Func<bool>> { HasResourceNodeTarget(), IsFalse(InventoryFull()) });

            // FROM 'move to resource' to ...
            AT(moveToResource, searchForResourceDrop, new List<Func<bool>> { AtDestination(), IsFalse(HasResourceDropTarget()), IsFalse(HasResourceNodeTarget()) });
            AT(moveToResource, harvestResource, new List<Func<bool>> { AtDestination(), HasResourceNodeTarget(), IsFalse(InventoryFull()) });

            // FROM 'harvest resource' to ...
            AT(harvestResource, searchForResourceDrop, new List<Func<bool>> { TargetIsDepletedAndICanCarryMore() });
            AT(harvestResource, moveToStockpile, new List<Func<bool>> { InventoryFull() });

            // FROM 'move to stockpile' to ...
            AT(moveToStockpile, placeItemsInStockpile, new List<Func<bool>> { ReachedStockpile() });

            // FROM 'place items in stockpile' to ...
            AT(placeItemsInStockpile, searchForResourceDrop, new List<Func<bool>> { () => gathered == 0 });

            // FROM 'flee' to ...
            //AT(flee, search, () => enemyDetector.EnemyInRange == false);

            /***************************************************************
             * FROM ANY STATE TRANSITIONS
            ***************************************************************/
            // TO 'return home' from *any state* when stuck
            stateMachine.AddFromAnyTransition(returnHome, new List<Func<bool>> { StuckOnMove() });

            // TO 'wander' from *any*
            stateMachine.AddFromAnyTransition(wander, new List<Func<bool>> { () => wanderCooldown > 30f });

            // TO 'flee' from *any state*
            //stateMachine.AddAnyTransition(flee, () => enemyDetector.EnemyInRange);

            /***************************************************************
             * TO ANY STATE TRANSITIONS ((use caution))
            ***************************************************************/
            // FROM 'wander' to *any state*
            stateMachine.AddToAnyTransition(wander);

            /**********************************************************************************************************************************************************************************
             * END TRANSITIONS
            ***********************************************************************************************************************************************************************************/

            // Set initial NPC state
            stateMachine.SetState(searchForResourceDrop);

            // AT(Add Transition) -- Internal function to provide a shorter name to declutter the transition list above (not technically needed)
            void AT(IState from, IState to, List<Func<bool>> conditions) => stateMachine.AddTransition(from, to, conditions);

            // Conditionals for transitions
            Func<bool> AtDestination() => () => Vector3.Distance(transform.position, destination) < 2f; ;
            Func<bool> HasResourceDropTarget() => () => resourceDropTarget != null;
            //Func<bool> NoResourceDropTarget() => () => resourceDropTarget == null;
            Func<bool> SearchAttempted() => () => searchForResourceDrop.attempted;
            Func<bool> HasResourceNodeTarget() => () => resourceNodeTarget != null;
            //Func<bool> ReachedResourceNode() => () => resourceNodeTarget != null && Vector3.Distance(transform.position, destination) < 3f;
            //Func<bool> ReachedResourceDrop() => () => Vector3.Distance(transform.position, destination) < 2f;
            Func<bool> TargetIsDepletedAndICanCarryMore() => () => harvestResource.depleted && !InventoryFull().Invoke();
            //Func<bool> InventoryNotFull() => () => gathered < maxCarried;
            Func<bool> InventoryFull() => () => gathered >= maxCarried;
            Func<bool> ReachedStockpile() => () => stockPile != null && Vector3.Distance(transform.position, navMeshAgent.destination) < 3f;
            
            Func<bool> IsFalse(Func<bool> conditionToInverse) => () => {
                if (conditionToInverse()) {
                    return false;
                } else {
                    return true;
                }
            };

            Func<bool> StuckOnMove() => () => {
                if (timeStuck > 5f) {
                    Debug.Log("I'm stuck!");
                    return true;
                } else {
                    return false;
                }
            };
        }

        private void Update() {
            updateCooldownTimers();
            stateMachine.Tick();
        }

        public override bool TakeFromTarget() {
            if (resourceNodeTarget.Hit(ItemStats.toolDamage)) {
                gathered++;

                if (debugLogs) {
                    Debug.Log("Gathered: " + gathered);
                }

                return true;
            } else {
                return false;
            }
        }

        public override bool Take() {
            if (gathered <= 0) {
                return false;
            } else {
                gathered--;
                return true;
            }
        }

        public override void DropAllResources() {
            
        }

        public override void ResetAgent() {
            animator.SetBool("move", false);
            atDestination = false;
        }

        private void updateCooldownTimers() {
            wanderCooldown += Time.deltaTime;
        }
    }
}

