using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    public class PickupItem : State {
        public override int priority => 15;
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
        }

        public override void Tick() {
            if (!finished) {
                if (Vector3.Distance(npc.transform.position, itemTarget.GetWorldPosition()) < 1f) {
                    if (itemTarget.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_ITEMPICKUP) && itemTarget.tileObject != null) {
                        if (npc.debugLogs) {
                            Debug.Log("PickupItem.Tick(): Picking up designated item");
                        }

                        if (typeof(BaseItem).IsInstanceOfType(itemTarget.tileObject)) {
                            BaseItem item = (BaseItem)itemTarget.tileObject;

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
                                        if (memoryKey.Contains(memoryTag) && Vector3.Distance(npc.transform.position, (Vector3)npc.memory.RetrieveMemory(memoryKey)) < 1f) {
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
                                Vector3Int tileMapPos = new Vector3Int(itemTarget.x, itemTarget.y, 0);
                                MapManager.Instance.tileMapList[4].SetTile(tileMapPos, null);

                                // Alter tile data
                                itemTarget.occupied = false;
                                itemTarget.occupiedStatus = ZetaUtilities.OCCUPIED_NONE;
                                itemTarget.occupiedCategory = ResourceCategory.None;
                                itemTarget.occupiedType = ResourceType.None;
                                itemTarget.tileObject = null;

                                // Finished
                                finished = true;
                                hasItemTarget = false;
                            }
                        } else {
                            Debug.LogWarning("Object is not an item that can be picked up");
                            finished = true;
                            hasItemTarget = false;
                        }
                    } else {
                        finished = true;
                        hasItemTarget = false;
                    }
                }
            }
        }
    }
}
