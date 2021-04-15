using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    /*
     * TODO:
     *  -add weight as a factor for inventory items
     *  
    */
    public class NpcInventory : MonoBehaviour {
        private Dictionary<BaseItem, int> inventory;
        private KeyValuePair<ResourceType, int> resourceCarried;
        private bool carryingSomething = false;
        private int maxInventoryCapacity = 10; // base this on a bag item
        private int maxStackAmount = 20; // change to per item basis?

        private void Awake() {
            inventory = new Dictionary<BaseItem, int>();
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

        public bool PickupResource(ResourceType resource) {
            // if already carrying something
            if (carryingSomething) {
                return false;
            } else {
                resourceCarried = new KeyValuePair<ResourceType, int>(resource, 1);
                carryingSomething = true;
                return true;
            }
        }

        public bool DropResource() {
            // if not carrying something, fail
            if (!carryingSomething) {
                return false;
            } else {
                carryingSomething = false;
                return true;
            }
        }

        public bool AddItem(BaseItem item) {
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
            foreach (BaseItem key in inventory.Keys) {
                Debug.Log("Item: " + key.ToString() + " || " + inventory[key]);
            }
        }
    }
}
