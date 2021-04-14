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

        private void Start() {
            // FORCED TEST PARAMETERS
            resourceNeeded = ResourceType.Wood;
            npcMemory.AddMemory("home", transform.position);

            // Create AI Personality
            personality = new Personality(PersonalityType.Default);

            // Create all NPC states
            var searchForResource = new SearchForResource(this);
            var harvestResource = new HarvestResource(this);
            var getUnstuck = new GetUnstuck(this);
            var wander = new Wander(this, personality.wanderRadius, personality.wanderCycle); // range and cycle time

            /***************************************************************
             * SPECIFIC STATE TRANSITIONS
            ***************************************************************/
            // FROM 'search for resource' to ...
            AT(searchForResource, harvestResource, new List<Func<bool>> { AtDestinationNotMoving(), HasResourceTarget(), IsFalse(InventoryFull()), () => resourceTarget.GetComponentInChildren<HarvestableResource>() != null });
          
            // FROM 'harvest resource' to ...
            AT(harvestResource, searchForResource, new List<Func<bool>> { IsFalse(HasResourceTarget()), IsFalse(InventoryFull()) });

            /***************************************************************
             * FROM ANY STATE TRANSITIONS
            ***************************************************************/
            // TO 'return home' from *any state* when stuck
            stateMachine.AddFromAnyTransition(getUnstuck, new List<Func<bool>> { StuckOnMove() });

            // TO 'wander' from *any*
            //stateMachine.AddFromAnyTransition(wander, new List<Func<bool>> { () => wanderCooldown > personality.wanderMaxCooldown });

            /***************************************************************
             * TO ANY STATE TRANSITIONS ((use caution as this opens a lot of unintended transitions))
            ***************************************************************/
            // FROM 'wander' to *any state*
            //stateMachine.AddToAnyTransition(wander);

            // FROM 'get unstuck' to *any state*
            stateMachine.AddToAnyTransition(getUnstuck);

            /**********************************************************************************************************************************************************************************
             * END TRANSITIONS
            ***********************************************************************************************************************************************************************************/

            // Set initial NPC state
            stateMachine.SetState(searchForResource);

            // AT(Add Transition) -- Internal function to provide a shorter name to declutter the transition list above (not technically needed)
            void AT(IState from, IState to, List<Func<bool>> conditions) => stateMachine.AddTransition(from, to, conditions);

            // Conditionals for transitions
            Func<bool> AtDestinationNotMoving() => () => Vector2.Distance(transform.position, destination) < 2f && animator.GetCurrentAnimatorStateInfo(0).IsTag("idle");
            Func<bool> HasResourceTarget() => () => resourceTarget != null;
            Func<bool> InventoryFull() => () => npcInventory.numWood >= npcInventory.maxInventoryCapacity;
            
            // Inverse a condition
            Func<bool> IsFalse(Func<bool> conditionToInverse) => () => {
                if (conditionToInverse()) {
                    return false;
                } else {
                    return true;
                }
            };

            // Stuck character timer
            Func<bool> StuckOnMove() => () => {
                if (timeStuck > 30f) {
                    Debug.Log("I'm stuck!");
                    return true;
                } else {
                    return false;
                }
            };
        }
    }
}

