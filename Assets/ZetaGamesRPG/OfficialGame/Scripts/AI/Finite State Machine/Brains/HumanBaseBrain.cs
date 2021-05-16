using System;
using System.Collections.Generic;
using UnityEngine;

/***********************************************
 * TODO:
 * -Implement a priority system for transitions in case there are multiple transitions
 *      that come back as true and possible. (Is this needed since I can prioritize on instantiation order?)
************************************************/

namespace ZetaGames.RPG {
    public class HumanBaseBrain : AIBrain {

        private void Start() {
            // FORCED TEST PARAMETERS

            // Create AI Personality
            personality = new Personality(PersonalityType.Default);

            // Create all NPC states
            buildStructure = new BuildStructure(this);
            searchForResource = new SearchForResource(this);
            harvestResource = new HarvestResource(this);
            storeResource = new StoreResource(this);
            pickupItem = new PickupItem(this);
            //var getUnstuck = new GetUnstuck(this);
            wander = new Wander(this);

            /***************************************************************
             * SPECIFIC STATE TRANSITIONS
            ***************************************************************/
            // FROM 'search for resource' to ...
            //AT(searchForResource, harvestResource, new List<Func<bool>> { HasResourceTarget(), IsFalse(IsInventoryFull()), IsFalse(IsCarryingResource()), IsResourceTargetHarvestable() });
           
            // FROM 'harvest resource' to ...
            //AT(harvestResource, searchForResource, new List<Func<bool>> { IsFalse(HasResourceTarget()), IsFalse(IsInventoryFull()), IsFalse(IsCarryingResource()) });

            /***************************************************************
             * FROM ANY STATE TRANSITIONS
            ***************************************************************/
            stateMachine.AddFromAnyTransition(buildStructure, new List<Func<bool>> { ReadyToBuild() });
            stateMachine.AddFromAnyTransition(searchForResource, new List<Func<bool>> { IsFalse(HasResourceTarget()), IsFalse(HasHarvestTarget()), IsFalse(InventoryFull()), ResourcesNeeded() });
            stateMachine.AddFromAnyTransition(harvestResource, new List<Func<bool>> { HasHarvestTarget(), IsFalse(InventoryFull()), IsHarvestTargetHarvestable() });
            stateMachine.AddFromAnyTransition(pickupItem, new List<Func<bool>> { IsFalse(InventoryFull()), HasItemTarget() });

            // TO 'store resource' from *any*
            stateMachine.AddFromAnyTransition(storeResource, new List<Func<bool>> { InventoryFull() });

            // TO 'return home' from *any state* when stuck
            //stateMachine.AddFromAnyTransition(getUnstuck, new List<Func<bool>> { StuckOnMove() });

            // TO 'wander' from *any*
            stateMachine.AddFromAnyTransition(wander, new List<Func<bool>> { () => wanderCooldown > personality.wanderMinCooldown });

            /***************************************************************
             * TO ANY STATE TRANSITIONS ((use caution as this opens a lot of unintended transitions))
            ***************************************************************/
            stateMachine.AddToAnyTransition(pickupItem);
            stateMachine.AddToAnyTransition(buildStructure);
            stateMachine.AddToAnyTransition(searchForResource);
            stateMachine.AddToAnyTransition(harvestResource);

            // FROM 'store resource' to *any state*
            stateMachine.AddToAnyTransition(storeResource);
            
            // FROM 'wander' to *any state*
            stateMachine.AddToAnyTransition(wander);

            // FROM 'get unstuck' to *any state*
            //stateMachine.AddToAnyTransition(getUnstuck);

            /**********************************************************************************************************************************************************************************
             * END TRANSITIONS
            ***********************************************************************************************************************************************************************************/

            // Set initial NPC state
            stateMachine.SetState(wander);

            // AT(Add Transition) -- Internal function to provide a shorter name to declutter the transition list above (not technically needed)
            //void AT(State from, State to, List<Func<bool>> conditions) => stateMachine.AddTransition(from, to, conditions);

            // Conditionals for transitions
            //Func<bool> AtDestinationStopped() => () => pathAgent.remainingDistance < 1f && pathAgent.isStopped;
            Func<bool> HasResourceTarget() => () => searchForResource.hasResourceTarget;
            Func<bool> HasHarvestTarget() => () => harvestResource.hasHarvestTarget;
            Func<bool> HasItemTarget() => () => pickupItem.hasItemTarget;
            Func<bool> IsHarvestTargetHarvestable() => () => harvestResource.harvestTarget.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_NODE_FULL);
            Func<bool> InventoryFull() => () => inventory.needToStoreItems == true;
            //Func<bool> IsCarryingResource() => () => inventory.IsCarryingSomething();
            Func<bool> ResourcesNeeded() => () => buildGoal.GetRequiredMaterials() != ResourceCategory.None;
            //Func<bool> NeedShelter() => () => needs.shelter < 100;
            //Func<bool> HasPlannedBuildGoal() => () => buildGoal.planned;
            Func<bool> ReadyToBuild() => () => buildGoal.IsReadyToBuild();
           
            // Inverse a condition
            Func<bool> IsFalse(Func<bool> conditionToInverse) => () => {
                if (conditionToInverse()) {
                    return false;
                } else {
                    return true;
                }
            };

            /*
            // Stuck character timer
            Func<bool> StuckOnMove() => () => {
                if (timeStuck > 30f) {
                    Debug.Log("I'm stuck!");
                    return true;
                } else {
                    return false;
                }
            };
            */
        }
    }
}

