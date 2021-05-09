using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    /*
     * TODO:
     *  -add weight as a factor for inventory items
     *  
    */
    public class NpcInventory : MonoBehaviour {
        private Dictionary<BaseItemData, int> inventory;
        private bool carryingSomething = false;
        private ResourceCategory carriedResourceCategory;
        private ResourceType carriedResourceType;
        private ResourceState carriedResourceState;
        private int maxInventoryCapacity = 10; // base this on a bag item
        private int maxStackAmount = 20; // change to per item basis?

        private void Awake() {
            inventory = new Dictionary<BaseItemData, int>();
        }

        public bool IsCarryingSomething() {
            if (carryingSomething) {
                return true;
            } else {
                return false;
            }
        }

        public bool IsInventoryFull() {
            if (inventory.Keys.Count >= maxInventoryCapacity) {
                PrintInventory();
                // inventory at max
                return true;
            } else {
                PrintInventory();
                // inventory below max
                return false;
            }
        }

        public bool PickupResource(ResourceCategory resourceCategory, ResourceType resourceType, ResourceState resourceState) {
            // if already carrying something
            if (carryingSomething) {
                return false;
            } else {
                carryingSomething = true;
                carriedResourceCategory = resourceCategory;
                carriedResourceType = resourceType;
                carriedResourceState = resourceState;
                return true;
            }
        }

        public bool DropResource() {
            // if not carrying something, fail
            if (!carryingSomething) {
                return false;
            } else {
                carryingSomething = false;
                carriedResourceCategory = ResourceCategory.None;
                carriedResourceType = ResourceType.None;
                carriedResourceState = ResourceState.None;
                return true;
            }
        }

        public bool AddItem(BaseItemData item) {
            // if similar item is already in inventory
            if (inventory.ContainsKey(item)) {
                // then add item to the stack if not over max
                if (inventory[item] >= maxStackAmount) {
                    PrintInventory();
                    return false;
                } else {
                    inventory[item]++;
                    PrintInventory();
                    return true;
                }
            } else {
                // add item to inventory if not at max bag capacity
                if (inventory.Count >= maxInventoryCapacity) {
                    PrintInventory();
                    return false;
                } else {
                    inventory.Add(item, 1);
                    PrintInventory();
                    return true;
                }
            }
        }

        public void PrintInventory() {
            foreach (BaseItemData key in inventory.Keys) {
                Debug.Log("Item: " + key.ToString() + " || " + inventory[key]);
            }
        }
    }
}
