using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    public class FindResource : State {
        public override int priority => 5;
        public override bool isFinished { get => finished; }
        public override bool isInterruptable { get => npc.inCombat; }

        private bool finished;
        private readonly AIBrain npc;
        private ResourceItem resourceItemData;
        private string lastKnownResourceNodeLocation;
        private bool activelySearching;
        private string memoryTag;
        private ResourceCategory resourceCategoryWanted;
        private ResourceType resourceTypeWanted;
        public WorldTile resourceTarget;
        public bool hasResourceTarget;

        public FindResource(AIBrain npc) {
            this.npc = npc;
        }

        public override void Tick() {
            if (!finished) {
                //Check for resources on timer to reduce CPU usage
                if (!activelySearching) {
                    // if we don't have a resource target, get one
                    if (resourceTarget == null && resourceCategoryWanted != ResourceCategory.None) {
                        GetTarget();
                    } else {
                        hasResourceTarget = false;
                        finished = true;
                    }
                } else if (activelySearching && npc.pathMovement.isStopped) {
                    activelySearching = false;
                }
            } else {
                Debug.LogWarning("SearchForResource.Tick(): Task finished, not doing anything.");
            }
        }

        public override void OnEnter() {
            resourceCategoryWanted = npc.buildGoal.GetRequiredMaterials();
            resourceTypeWanted = npc.buildGoal.GetRequiredResType(resourceCategoryWanted);
            lastKnownResourceNodeLocation = "last" + resourceTypeWanted + resourceCategoryWanted.ToString() + "Node";
            resourceTarget = null;
            activelySearching = false;
            memoryTag = null;
            finished = false;
            hasResourceTarget = false;

            if (npc.debugLogs) {
                Debug.Log("SearchForResource.OnEnter(): Resources Needed: " + npc.buildGoal.GetResourceAmount(npc.buildGoal.GetRequiredMaterials()));
            }
        }

        public override void OnExit() {
            resourceItemData = null;
            resourceTarget = null;
        }

        private void GetTarget() {
            if (npc.debugLogs) {
                Debug.Log("SearchForResource.GetTarget(): Getting resource target");
            }

            WorldTile closestTile = null;
            WorldTile[] originRegionTiles = MapManager.Instance.GetWorldRegionGrid().GetGridObject(npc.stats.settlement.originRegion.x, npc.stats.settlement.originRegion.y);
            Vector3 currentPos = npc.transform.position;
            Vector3 homePos = (npc.stats.settlement != null) ? originRegionTiles[Random.Range(0, originRegionTiles.Length)].GetWorldPosition() : npc.buildGoal.buildSiteLocation;
           
            ZetaGrid<WorldTile> mapGrid = MapManager.Instance.GetWorldTileGrid();
            int mapWidth = MapManager.Instance.mapWidth;
            int mapHeight = MapManager.Instance.mapHeight;

            // search for dropped resources
            for (int x = 0; x < 11; x++) {
                for (int y = 0; y < 11; y++) {
                    // check grid bounds
                    if ((int)currentPos.x + (x - 5) < mapWidth && (int)currentPos.y + (y - 5) < mapHeight && (int)currentPos.x + (x - 5) >= 0 && (int)currentPos.y + (y - 5) >= 0) {
                        WorldTile tile = mapGrid.GetGridObject((int)currentPos.x + (x - 5), (int)currentPos.y + (y - 5));
                        if (tile.occupied && tile.tileObstacle != null && tile.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_ITEMPICKUP)) {
                            if (typeof(ResourceItem).IsInstanceOfType(tile.tileObstacle)) {
                                resourceItemData = (ResourceItem)tile.tileObstacle;
                                if (resourceItemData.resourceCategory == resourceCategoryWanted) {
                                    // If inventory is not full of that specific type of resource
                                    if (!npc.inventory.IsInventoryFull(resourceItemData)) {
                                        npc.inventory.needToStoreItems = false;
                                        // If we're not looking for a specific type of resource, then match the type we found
                                        if (resourceTypeWanted == ResourceType.None) {
                                            resourceTypeWanted = resourceItemData.resourceType;
                                        }

                                        if (resourceItemData.resourceType == resourceTypeWanted) {
                                            resourceTarget = tile;
                                            break;
                                        }
                                    } else {
                                        npc.inventory.needToStoreItems = true;
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }

                if (resourceTarget != null) {
                    break;
                }
            }

            // if a dropped resource is found, set the destination
            if (resourceTarget != null) {
                if (npc.debugLogs) {
                    Debug.Log("SearchForResourceDrop.Tick(): dropped resource found");
                }

                activelySearching = true;
                npc.pickupItem.hasItemTarget = true;
                npc.pickupItem.itemTarget = resourceTarget;
                npc.pathMovement.destination = resourceTarget.GetWorldPosition() + MapManager.Instance.GetTileOffset();
                npc.pathMovement.SearchPath();

                // Finished
                finished = true;
                resourceTarget = null;
            } else {
                // retrieve memory of a dropped item if there was one and it's close by
                if (resourceTypeWanted == ResourceType.None) {
                    memoryTag = resourceCategoryWanted.ToString() + ZetaUtilities.OCCUPIED_ITEMPICKUP;
                } else {
                    memoryTag = resourceTypeWanted.ToString() + resourceCategoryWanted.ToString() + ZetaUtilities.OCCUPIED_ITEMPICKUP;
                }

                foreach (var memoryKey in npc.memory.GetAllMemories().Keys) {
                    if (memoryKey != null) {
                        if (memoryKey.Contains(memoryTag) && Vector3.Distance(npc.transform.position, (Vector3)npc.memory.RetrieveMemory(memoryKey)) < 60f) {
                            if (npc.debugLogs) {
                                Debug.Log("SearchForResource.GetTarget(): Remembered resource drop and moving towards");
                            }

                            activelySearching = true;
                            npc.pathMovement.destination = (Vector3)npc.memory.RetrieveMemory(memoryKey);
                            npc.pathMovement.SearchPath();
                            npc.pickupItem.hasItemTarget = true;
                            npc.pickupItem.itemTarget = mapGrid.GetGridObject((int)npc.pathMovement.destination.x, (int)npc.pathMovement.destination.y);

                            // Finished
                            finished = true;
                            resourceTarget = null;
                            return;
                        }
                    }
                }

                // look for a resource node
                int adjGridSize = 1;
                int step = 0;

                for (int i = 0; i < 14; i++) {
                    adjGridSize += 4;
                    step += 2;

                    for (int x = 0; x < adjGridSize; x++) {
                        for (int y = 0; y < adjGridSize; y++) {
                            // check grid bounds
                            if (homePos.x + (x - step) < mapWidth && homePos.y + (y - step) < mapHeight && homePos.x + (x - step) >= 0 && homePos.y + (y - step) >= 0) {
                                WorldTile tile = mapGrid.GetGridObject((int)homePos.x + (x - step), (int)homePos.y + (y - step));
                                if (tile.occupied) {
                                    if (tile.tileObstacle != null && tile.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_NODE_FULL)) {
                                        if (typeof(ResourceNode).IsInstanceOfType(tile.tileObstacle)) {
                                            ResourceNode resourceNode = (ResourceNode)tile.tileObstacle;
                                            if (resourceNode.resourceCategory == resourceCategoryWanted) {
                                                // If inventory is not full of that specific type of resource
                                                if (!npc.inventory.IsInventoryFull(resourceNode.resourceItemData)) {
                                                    npc.inventory.needToStoreItems = false;

                                                    if (resourceTypeWanted == ResourceType.None) {
                                                        resourceTypeWanted = resourceNode.resourceType;
                                                    }

                                                    if (resourceNode.resourceType == resourceTypeWanted && tile.lockTag == -1) {
                                                        if (closestTile == null) {
                                                            closestTile = tile;
                                                        } else if (Vector3.Distance(tile.GetWorldPosition(), currentPos) < Vector3.Distance(closestTile.GetWorldPosition(), currentPos)) {
                                                            // If the distance between the targeted resource tile and the current position is less than the current closest tile and the current position, then make this the closest
                                                            closestTile = tile;
                                                        }
                                                    }
                                                } else {
                                                    npc.inventory.needToStoreItems = true;
                                                    return;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (closestTile != null) {
                            break;
                        }
                    }
                }

                // if a resource node was found, target the closest one found
                if (closestTile != null) {
                    if (npc.debugLogs) {
                        Debug.Log("SearchForResourceDrop.Tick(): resource node found");
                    }

                    activelySearching = true;
                    closestTile.lockTag = npc.lockTag;
                    npc.memory.AddMemory(lastKnownResourceNodeLocation, closestTile.GetWorldPosition());
                    npc.harvestResource.harvestTarget = closestTile;
                    npc.harvestResource.hasHarvestTarget = true;

                    // Finished
                    finished = true;
                } else {
                    // otherwise, travel to last known location of the resource if there is one remembered and not too far away
                    if (resourceTarget == null) {
                        if (npc.memory.ContainsMemory(lastKnownResourceNodeLocation)) {
                            if (npc.debugLogs) {
                                Debug.Log("SearchForResourceDrop.Tick(): memory found");
                            }

                            Vector3 memoryLocation = (Vector3)npc.memory.RetrieveMemory(lastKnownResourceNodeLocation);

                            mapGrid.GetXY(currentPos, out int curX, out int curY);
                            mapGrid.GetXY(memoryLocation, out int memX, out int memY);

                            if (Mathf.Abs(memX - curX) < npc.personality.maxDistanceFromPosition && Mathf.Abs(memY - curY) < npc.personality.maxDistanceFromPosition) {
                                activelySearching = true;
                                npc.pathMovement.destination = memoryLocation;
                                npc.pathMovement.SearchPath();
                                npc.memory.RemoveMemory(lastKnownResourceNodeLocation);
                            }
                        } else {
                            // DEBUG - No trees in search range. Walk 30 tiles in a random direction to find some more.
                            if (npc.debugLogs) {
                                Debug.Log("SearchForResourceDrop.Tick(): No resources found. Wandering until I find some.");
                            }

                            Vector3 destination = new Vector3(npc.transform.position.x + Random.Range(-30f, 30f), npc.transform.position.y + Random.Range(-30f, 30f));

                            if (destination.x < mapWidth && destination.y < mapHeight && destination.x >= 0 && destination.y >= 0) {
                                WorldTile destinationTile = mapGrid.GetGridObject((int)destination.x, (int)destination.y);
                                if (destinationTile.walkable) {
                                    npc.pathMovement.destination = destination;
                                    npc.pathMovement.SearchPath();
                                }
                            }

                            activelySearching = true;
                        }
                    }
                }
            }
        }
    }
}

