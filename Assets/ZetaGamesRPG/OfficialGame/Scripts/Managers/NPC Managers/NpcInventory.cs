using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    public class NpcInventory {
        // An inventory slot struct
        private struct InventorySlot {
            public BaseItem item;
            public int amount;
        }

        private InventorySlot[] inventorySlots;
        private int maxInventoryCapacity = 2; // base this on a bag item
        public bool needToStoreItems { get; set; }

        public NpcInventory() {
            // Initialize Inventory
            inventorySlots = new InventorySlot[10]; // 10 is the largest the inventory will ever be
        }

        public int AddItem(BaseItem item, int amountToAdd) {
            // Look through each inventory slot for existing item
            for (int i = 0; i < maxInventoryCapacity; i++) {
                // If there is an item in the slot with the same name
                if (inventorySlots[i].item != null) {
                    if (inventorySlots[i].item.itemName.Equals(item.itemName)) {
                        // If the current amount is not at the max stack cap, then add to stack
                        if (inventorySlots[i].amount < item.maxInventoryStack) {
                            inventorySlots[i].amount += amountToAdd;
                            // Check to make sure it didn't go over the max stack amount
                            if (inventorySlots[i].amount > item.maxInventoryStack) {
                                // Change amountToAdd to the leftover amount
                                amountToAdd = inventorySlots[i].amount - item.maxInventoryStack;
                            } else {
                                amountToAdd = 0;
                                return amountToAdd;
                            }
                        }
                    }
                }
            }

            // Fill an empty inventory slot with remaining amount
            for (int i = 0; i < maxInventoryCapacity; i++) {
                // If there is an empty slot
                if (inventorySlots[i].item == null) {
                    inventorySlots[i].item = item;
                    inventorySlots[i].amount = amountToAdd;
                    // Check to make sure it didn't go over the max stack amount
                    if (inventorySlots[i].amount > item.maxInventoryStack) {
                        // Change amountToAdd to the leftover amount
                        amountToAdd = inventorySlots[i].amount - item.maxInventoryStack;
                    } else {
                        amountToAdd = 0;
                        return amountToAdd;
                    }
                }
            }

            // return the leftover amount (inventory full)
            return amountToAdd;
        }

        public bool IsInventoryFull(BaseItem item) {
            // Look through each inventory slot for existing item
            for (int i = 0; i < maxInventoryCapacity; i++) {
                // If there is an item in the slot with the same name
                if (inventorySlots[i].item != null) {
                    if (inventorySlots[i].item.itemName.Equals(item.itemName)) {
                        // If the current amount is not at the max stack cap
                        if (inventorySlots[i].amount < item.maxInventoryStack) {
                            return false;
                        }
                    }
                } else {
                    return false;
                }
            }

            // No empty spots and/or existing items at max stack capacity
            return true;
        }

        public int GetAmountOfResource(ResourceCategory resourceCategory) {
            int amount = 0;
            
            for (int i = 0; i < maxInventoryCapacity; i++) {
                if (inventorySlots[i].item != null) {
                    if (typeof(ResourceItem).IsInstanceOfType(inventorySlots[i].item)) {
                        ResourceItem resourceItem = (ResourceItem)inventorySlots[i].item;
                        if (resourceItem.resourceCategory.Equals(resourceCategory)) {
                            amount += inventorySlots[i].amount;
                        }
                    }
                }
            }

            return amount;
        }

        /*
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
        */

        public void PrintInventory() {
            for (int i = 0; i < inventorySlots.Length; i++) {
                Debug.Log("Slot: " + i + " || Item: " + inventorySlots[i].item.ToString() + " || Amount: " + inventorySlots[i].amount);
            }
        }
    }
}
