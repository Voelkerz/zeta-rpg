using UnityEngine;

namespace ZetaGames.RPG {
    public class SearchForResource : IState {
        public bool isFinished { get => finished; }
        public bool isInterruptable { get => true; }
        private bool finished;
        private readonly AIBrain npcBrain;
        private ResourceDropData resourceDropData;
        private float tickTimer;
        private string lastKnownResourceNodeLocation;
        private bool activelySearching;
        private Vector3Int tileMapPos;

        public SearchForResource(AIBrain npcBrain) {
            this.npcBrain = npcBrain;
        }

        public void Tick() {
            tickTimer += npcBrain.deltaTime;

            //Check for resources on timer to reduce CPU usage
            if (tickTimer > 0.5f && !activelySearching) {
                tickTimer = 0;
                // if we don't have a resource target, get one
                if (npcBrain.resourceTileTarget == null) {
                    GetTarget();
                } else if (npcBrain.resourceTileTarget.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_ITEMPICKUP) && resourceDropData != null) {
                    // If our target tile is a resource item drop, then go pick it up
                    if (resourceDropData.resourceCategory == npcBrain.resourceCategoryWanted && resourceDropData.resourceType == npcBrain.resourceTypeWanted && Vector3.Distance(npcBrain.transform.position, npcBrain.resourceTileTarget.GetWorldPosition()) <= 1) {
                        if (npcBrain.debugLogs) {
                            Debug.Log("SearchForResourceDrop.Tick(): Picking up dropped resource");
                        }

                        // Pickup item
                        npcBrain.npcInventory.PickupResource(resourceDropData.resourceCategory, resourceDropData.resourceType, resourceDropData.resourceState);

                        // Nullify tilemap sprite of item
                        Vector3 tileWorldPos = npcBrain.resourceTileTarget.GetWorldPosition();
                        tileMapPos.x = (int)tileWorldPos.x;
                        tileMapPos.y = (int)tileWorldPos.y;
                        tileMapPos.z = 0;
                        MapManager.Instance.tileMapList[4].SetTile(tileMapPos, null);

                        // Alter tile data
                        npcBrain.resourceTileTarget.occupied = false;
                        npcBrain.resourceTileTarget.occupiedStatus = ZetaUtilities.OCCUPIED_NONE;
                        npcBrain.resourceTileTarget.occupiedCategory = ResourceCategory.None;
                        npcBrain.resourceTileTarget.occupiedType = ResourceType.None;
                        npcBrain.resourceTileTarget.tileObjectData = null;
                        npcBrain.resourceTileTarget = null;
                        resourceDropData = null;
                    }
                }
            } else if (activelySearching && npcBrain.pathMovement.isStopped) {
                activelySearching = false;
            }
        }

        public void OnEnter() {
            if (npcBrain.debugLogs) {
                Debug.Log("SearchForResource.OnEnter()");
            }
            tickTimer = 0;
            lastKnownResourceNodeLocation = "last" + npcBrain.resourceTypeWanted + npcBrain.resourceCategoryWanted.ToString() + "Node";
            npcBrain.resourceTileTarget = null;
            activelySearching = false;
        }

        public void OnExit() {
            resourceDropData = null;
        }

        private void GetTarget() {
            if (npcBrain.debugLogs) {
                Debug.Log("SearchForResource.GetTarget(): Getting resource target");
            }

            WorldTile closestTile = null;
            Vector3 currentPos = npcBrain.transform.position;
            ZetaGrid<WorldTile> mapGrid = MapManager.Instance.GetWorldTileGrid();
            int mapWidth = MapManager.Instance.mapWidth;
            int mapHeight = MapManager.Instance.mapHeight;

            // search for dropped resources
            for (int x = 0; x < 7; x++) {
                for (int y = 0; y < 7; y++) {
                    // check grid bounds
                    if ((int)currentPos.x + (x - 3) < mapWidth && (int)currentPos.y + (y - 3) < mapHeight && (int)currentPos.x + (x - 3) >= 0 && (int)currentPos.y + (y - 3) >= 0) {
                        WorldTile tile = mapGrid.GetGridObject((int)currentPos.x + (x - 3), (int)currentPos.y + (y - 3));
                        if (tile.occupied && tile.tileObjectData != null && tile.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_ITEMPICKUP)) {
                            if (typeof(ResourceDropData).IsInstanceOfType(tile.tileObjectData)) {
                                resourceDropData = (ResourceDropData)tile.tileObjectData;
                                if (resourceDropData.resourceCategory == npcBrain.resourceCategoryWanted && resourceDropData.resourceType == npcBrain.resourceTypeWanted) {
                                    npcBrain.resourceTileTarget = tile;
                                    break;
                                }
                            }
                        }
                    }
                }

                if (npcBrain.resourceTileTarget != null) {
                    break;
                }
            }

            // if a dropped resource is found, set the destination
            if (npcBrain.resourceTileTarget != null) {
                if (npcBrain.debugLogs) {
                    Debug.Log("SearchForResourceDrop.Tick(): dropped resource found");
                }

                activelySearching = true;
                npcBrain.pathMovement.destination = npcBrain.resourceTileTarget.GetWorldPosition() + MapManager.Instance.GetTileOffset();
                npcBrain.pathMovement.SearchPath();
            } else {
                // look for a resource node
                int adjGridSize = 1;
                int step = 0;

                for (int i = 0; i < 14; i++) {
                    adjGridSize += 4;
                    step += 2;

                    for (int x = 0; x < adjGridSize; x++) {
                        for (int y = 0; y < adjGridSize; y++) {
                            // check grid bounds
                            if (currentPos.x + (x - step) < mapWidth && currentPos.y + (y - step) < mapHeight && currentPos.x + (x - step) >= 0 && currentPos.y + (y - step) >= 0) {
                                WorldTile tile = mapGrid.GetGridObject((int)currentPos.x + (x - step), (int)currentPos.y + (y - step));
                                if (tile.occupied) {
                                    if (tile.tileObjectData != null && tile.occupiedStatus.Equals(ZetaUtilities.OCCUPIED_NODE_FULL)) {
                                        if (tile.occupiedCategory == npcBrain.resourceCategoryWanted && tile.occupiedType == npcBrain.resourceTypeWanted) {
                                            if (closestTile == null) {
                                                closestTile = tile;
                                            } else if (Vector3.Distance(tile.GetWorldPosition(), currentPos) < Vector3.Distance(closestTile.GetWorldPosition(), currentPos)) {
                                                // If the distance between the targeted resource tile and the current position is less than the current closest tile and the current position, then make this the closest
                                                closestTile = tile;
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


                // if a resource node was found, target the closest one found
                if (closestTile != null) {
                    if (npcBrain.debugLogs) {
                        Debug.Log("SearchForResourceDrop.Tick(): resource node found");
                    }

                    closestTile.lockTag = npcBrain.npcLockTag;
                    npcBrain.npcMemory.AddMemory(lastKnownResourceNodeLocation, closestTile.GetWorldPosition());
                    npcBrain.resourceTileTarget = closestTile;
                } else {
                    // otherwise, travel to last known location of the resource if there is one remembered and not too far away
                    if (npcBrain.resourceTileTarget == null) {
                        if (npcBrain.npcMemory.ContainsMemory(lastKnownResourceNodeLocation)) {
                            if (npcBrain.debugLogs) {
                                Debug.Log("SearchForResourceDrop.Tick(): memory found");
                            }

                            Vector3 memoryLocation = (Vector3)npcBrain.npcMemory.RetrieveMemory(lastKnownResourceNodeLocation);

                            mapGrid.GetXY(currentPos, out int curX, out int curY);
                            mapGrid.GetXY(memoryLocation, out int memX, out int memY);

                            if (Mathf.Abs(memX - curX) < npcBrain.personality.maxDistanceFromPosition && Mathf.Abs(memY - curY) < npcBrain.personality.maxDistanceFromPosition) {
                                activelySearching = true;
                                npcBrain.pathMovement.destination = memoryLocation;
                                npcBrain.pathMovement.SearchPath();
                                npcBrain.npcMemory.RemoveMemory(lastKnownResourceNodeLocation);
                            }
                        } else {
                            // DEBUG - No trees in sight, so stop trying to search for them
                            npcBrain.npcInventory.PickupResource(npcBrain.resourceCategoryWanted, npcBrain.resourceTypeWanted, ResourceState.Raw);
                        }
                    }
                }
            }
        }
    }
}

