using System;
using System.Collections.Generic;

namespace ZetaGames.RPG {
    public class HumanBaseBrain : AIBrain {
        
        private void Start() {
            // Create AI Personality
            personality = new Personality(PersonalityType.Default);

            // Create all NPC states
            joinCommunity = new JoinCommunity(this);
            buildStructure = new BuildStructure(this);
            findResource = new FindResource(this);
            harvestResource = new HarvestResource(this);
            storeResource = new StoreResource(this);
            pickupItem = new PickupItem(this);
            wander = new Wander(this);

            /********************************************************************
             * FROM ANY STATE TRANSITIONS (To this transition from any other)
            *********************************************************************/
            stateMachine.AddFromAnyTransition(joinCommunity, new List<Func<bool>> { IsFalse(HasCommunity()), () => communityScore >= 100  });
            stateMachine.AddFromAnyTransition(buildStructure, new List<Func<bool>> { HasPlannedBuildGoal(), CanMakeBuildProgress() });
            stateMachine.AddFromAnyTransition(findResource, new List<Func<bool>> { IsFalse(HasHarvestTarget()), IsFalse(InventoryFull()), ResourcesNeeded() });
            stateMachine.AddFromAnyTransition(harvestResource, new List<Func<bool>> { HasHarvestTarget(), IsFalse(InventoryFull()), IsHarvestTargetHarvestable() });
            stateMachine.AddFromAnyTransition(pickupItem, new List<Func<bool>> { IsFalse(InventoryFull()), HasItemTarget() });
            stateMachine.AddFromAnyTransition(storeResource, new List<Func<bool>> { InventoryFull() });
            stateMachine.AddFromAnyTransition(wander, new List<Func<bool>> { () => wanderCooldown > personality.wanderMinCooldown });

            /********************************************************************
             * TO ANY STATE TRANSITIONS (From this transition to any other)
            *********************************************************************/
            stateMachine.AddToAnyTransition(joinCommunity);
            stateMachine.AddToAnyTransition(pickupItem);
            stateMachine.AddToAnyTransition(buildStructure);
            stateMachine.AddToAnyTransition(findResource);
            stateMachine.AddToAnyTransition(harvestResource);
            stateMachine.AddToAnyTransition(storeResource);
            stateMachine.AddToAnyTransition(wander);

            /**********************************************************************************************************************************************************************************
             * END TRANSITIONS
            ***********************************************************************************************************************************************************************************/

            // Set initial NPC state
            stateMachine.SetState(wander);

            // Conditionals for transitions
            Func<bool> HasHarvestTarget() => () => harvestResource.hasHarvestTarget;
            Func<bool> HasItemTarget() => () => pickupItem.hasItemTarget;
            Func<bool> IsHarvestTargetHarvestable() => () => harvestResource.harvestTarget.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_NODE_FULL);
            Func<bool> InventoryFull() => () => inventory.needToStoreItems == true;
            Func<bool> ResourcesNeeded() => () => buildGoal.GetRequiredMaterials() != ResourceCategory.None;
            Func<bool> HasPlannedBuildGoal() => () => buildGoal.hasBuildGoal;
            Func<bool> CanMakeBuildProgress() => () => (buildGoal.HasRequiredMaterialsInInventory() && (inventory.needToStoreItems || inventory.GetAmountOfResource(buildGoal.GetRequiredMaterials()) == buildGoal.GetResourceAmount(buildGoal.GetRequiredMaterials()))) || !buildGoal.hasBuildSite;
            Func<bool> HasCommunity() => () => joinCommunity.hasCommunity;

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

            // Pick a profession
            PickProfession();
        }

        protected override void Update() {
            base.Update();
        }

        public override void JoinCommunity() {
            
        }

        public override void PickProfession() {
            
        }
    }
}

