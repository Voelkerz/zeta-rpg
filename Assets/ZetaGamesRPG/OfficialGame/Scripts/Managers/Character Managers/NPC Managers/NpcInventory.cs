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

        public void RemoveItem(BaseItem item, int amountToRemove) {

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
                                // Check to see if inventory is full of this resource
                                needToStoreItems = IsInventoryFull(item);

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
                        // Check to see if inventory is full of this resource
                        needToStoreItems = IsInventoryFull(item);

                        amountToAdd = 0;
                        return amountToAdd;
                    }
                }
            }
            
            // return the leftover amount (inventory full)
            needToStoreItems = IsInventoryFull(item);
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

        public bool IsInventoryFullOfResource(ResourceCategory resourceCategory) {
            bool inventoryFull = false;
            
            for (int i = 0; i < maxInventoryCapacity; i++) {
                if (inventorySlots[i].item != null) {
                    if (typeof(ResourceItem).IsInstanceOfType(inventorySlots[i].item)) {
                        ResourceItem resourceItem = (ResourceItem)inventorySlots[i].item;
                        if (resourceItem.resourceCategory.Equals(resourceCategory)) {
                            inventoryFull = IsInventoryFull(resourceItem);
                        }
                    }
                }
            }

            return inventoryFull;
        }

        public int RemoveResource(ResourceCategory resourceCategory, int amountToRemove) {
            for (int i = 0; i < maxInventoryCapacity; i++) {
                if (inventorySlots[i].item != null) {
                    if (typeof(ResourceItem).IsInstanceOfType(inventorySlots[i].item)) {
                        ResourceItem resourceItem = (ResourceItem)inventorySlots[i].item;
                        if (resourceItem.resourceCategory.Equals(resourceCategory)) {
                            inventorySlots[i].amount -= amountToRemove;
                            
                            if (inventorySlots[i].amount < 0) {
                                // change to leftovers
                                amountToRemove = Mathf.Abs(inventorySlots[i].amount);
                                
                                // nullify empty slot
                                inventorySlots[i].amount = 0;
                                inventorySlots[i].item = null;

                                // use recursion to remove more from other slots
                                RemoveResource(resourceCategory, amountToRemove);
                            } else if (inventorySlots[i].amount == 0) {
                                inventorySlots[i].item = null;
                                return 0;
                            } else {
                                return 0;
                            }
                        }
                    }
                }
            }

            // return what couldn't be removed
            return amountToRemove;
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
            for (int i = 0; i < maxInventoryCapacity; i++) {
                if (inventorySlots[i].item != null) {
                    Debug.Log("Slot: " + i + " || Item: " + inventorySlots[i].item.itemName + " || Amount: " + inventorySlots[i].amount);
                }
            }
        }
    }
}
