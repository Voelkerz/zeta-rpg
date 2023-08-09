using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    public class PickupItem : State {
        public override float actionScore { get; set; } = 15;
        public override bool isFinished => finished;
        public override bool isInterruptable => npc.inCombat;

        private bool finished;
        private readonly AIBrain npc;
        public bool hasItemTarget;
        public WorldTile itemTarget;
        private int itemAmount = 1;

        public PickupItem(AIBrain npc) {
            this.npc = npc;
        }

        public override void OnEnter() {
            finished = false;

            if (!hasItemTarget) {
                finished = true;
            }
        }

        public override void OnExit() {
            itemTarget = null;
            hasItemTarget = false;
        }

        public override void Tick() {
            if (!finished) {
                if (Vector3.Distance(npc.transform.position, itemTarget.GetWorldPosition()) < 1f) {
                    if (itemTarget.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_ITEMPICKUP) && itemTarget.tileObstacle != null) {
                        if (npc.debugLogs) {
                            Debug.Log("PickupItem.Tick(): Picking up designated item");
                        }

                        if (typeof(BaseItem).IsInstanceOfType(itemTarget.tileObstacle)) {
                            BaseItem item = (BaseItem)itemTarget.tileObstacle;

                            // Pickup item
                            int leftovers = npc.inventory.AddItem(item, itemAmount);

                            npc.inventory.PrintInventory();

                            if (leftovers > 0) {
                                // If there are leftovers, NPC needs to store items to make room
                                npc.inventory.needToStoreItems = true;

                                // If leftovers does not equal the amount picked up, it means some were picked up
                                if (leftovers != itemAmount) {
                                    // Subtract amount that was left behind
                                    itemAmount -= leftovers;



                                    //TODO: Add code to put the leftover itemAmount back on the ground.



                                    // Finished
                                    finished = true;
                                } else {
                                    // Leftover equalled the amount picked up, which means nothing was picked up.
                                    finished = true;

                                    if (npc.debugLogs) {
                                        Debug.Log("PickupItem.Tick(): Item(s) not picked up, inventory is full.");
                                    }
                                }

                            } else {
                                // no leftovers. remove sprite and alter tile data

                                // remove memory if there was one
                                string memoryTag = item.name + ZetaUtilities.OCCUPIED_ITEMPICKUP;
                                string memory = null;

                                foreach (string memoryKey in npc.memory.GetAllMemories().Keys) {
                                    if (memoryKey != null) {
                                        if (memoryKey.Contains(memoryTag) && Vector3.Distance(npc.transform.position, (Vector3)npc.memory.RetrieveMemory(memoryKey)) < 0.5f) {
                                            memory = memoryKey;
                                        }
                                    }
                                }

                                if (memory != null) {
                                    npc.memory.RemoveMemory(memory);

                                    if (npc.debugLogs) {
                                        Debug.Log("PickupItem.Tick(): Memory of picked up item found and being removed");
                                    }
                                }

                                // Nullify tilemap sprite of item
                                MapManager.Instance.tileMapList[ZetaUtilities.TILEMAP_ITEM_DROP + itemTarget.elevation].SetTile(itemTarget.GetWorldPositionInt(), null);

                                // Alter tile data
                                //itemTarget.ResetTile();
                                itemTarget.occupied = false;
                                itemTarget.occupiedStatus = ZetaUtilities.OCCUPIED_NONE;
                                itemTarget.tileObstacle = null;

                                // Finished
                                finished = true;
                            }
                        } else {
                            Debug.LogWarning("Object is not an item that can be picked up");
                            finished = true;
                        }
                    } else {
                        string key = null;

                        // remove memory if there was one and item is already gone
                        foreach (string memoryKey in npc.memory.GetAllMemories().Keys) {
                            if (memoryKey != null) {
                                if (Vector3.Distance(npc.transform.position, (Vector3)npc.memory.RetrieveMemory(memoryKey)) < 1f) {
                                    key = memoryKey;
                                }
                            }
                        }

                        if (key != null) {
                            if (npc.debugLogs) {
                                Debug.Log("PickupItem.Tick(): No item to pickup, but memory found.");
                            }
                            npc.memory.RemoveMemory(key);
                        }
                        
                        finished = true;
                    }
                }
            }
        }
    }
}
