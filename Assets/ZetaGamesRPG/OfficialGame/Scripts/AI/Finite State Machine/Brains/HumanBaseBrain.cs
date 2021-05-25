using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ZetaGames.RPG {
    public class HumanBaseBrain : AIBrain {
        
        private void Start() {
            // Create AI Personality
            personality = new Personality(PersonalityType.Default);

            // Create all NPC states
            buildStructure = new BuildStructure(this);
            searchForResource = new SearchForResource(this);
            harvestResource = new HarvestResource(this);
            storeResource = new StoreResource(this);
            pickupItem = new PickupItem(this);
            wander = new Wander(this);

            /********************************************************************
             * FROM ANY STATE TRANSITIONS (To this transition from any other)
            *********************************************************************/
            stateMachine.AddFromAnyTransition(buildStructure, new List<Func<bool>> { HasPlannedBuildGoal(), CanMakeBuildProgress() });
            stateMachine.AddFromAnyTransition(searchForResource, new List<Func<bool>> { IsFalse(HasHarvestTarget()), IsFalse(InventoryFull()), ResourcesNeeded() });
            stateMachine.AddFromAnyTransition(harvestResource, new List<Func<bool>> { HasHarvestTarget(), IsFalse(InventoryFull()), IsHarvestTargetHarvestable() });
            stateMachine.AddFromAnyTransition(pickupItem, new List<Func<bool>> { IsFalse(InventoryFull()), HasItemTarget() });
            stateMachine.AddFromAnyTransition(storeResource, new List<Func<bool>> { InventoryFull() });
            stateMachine.AddFromAnyTransition(wander, new List<Func<bool>> { () => wanderCooldown > personality.wanderMinCooldown });

            /********************************************************************
             * TO ANY STATE TRANSITIONS (From this transition to any other)
            *********************************************************************/
            stateMachine.AddToAnyTransition(pickupItem);
            stateMachine.AddToAnyTransition(buildStructure);
            stateMachine.AddToAnyTransition(searchForResource);
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

            // Join a community
            JoinCommunity();

            // Pick a profession
            PickProfession();
        }

        protected override void Update() {
            base.Update();
        }

        public override void JoinCommunity() {
            // Look through the list of settlements and randomly choose a settlement to join
            if (CommunityManager.Instance.settlementList.Count > 0) {
                if (Random.Range(0, 100f) <= personality.startCommunityChance && CommunityManager.Instance.viableRegions.Count > 0) {
                    stats.settlement = CommunityManager.Instance.CreateSettlement(gameObject);
                    Debug.Log("Creating a new settlement: " + stats.settlement.settlementName + " || Viable Regions: " + CommunityManager.Instance.viableRegions.Count);
                } else {
                    stats.settlement = CommunityManager.Instance.JoinRandomSettlement(gameObject);
                    //Debug.Log("Joining settlement: " + stats.settlement.settlementName + " || Population: " + stats.settlement.citizenList.Count);
                }
            } else {
                stats.settlement = CommunityManager.Instance.CreateSettlement(gameObject);
                Debug.Log("Creating first settlement: " + stats.settlement.settlementName + " || Viable Regions: " + CommunityManager.Instance.viableRegions.Count);
            }
        }

        public override void PickProfession() {
            
        }
    }
}

