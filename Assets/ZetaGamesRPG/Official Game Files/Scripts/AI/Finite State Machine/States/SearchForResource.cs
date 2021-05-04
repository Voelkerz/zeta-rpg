using Pathfinding;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    public class SearchForResource : IState {
        public bool isFinished { get => finished; }
        public bool isInterruptable { get => true; }
        private bool finished;
        private readonly AIBrain npcBrain;
        private float tickTimer;
        private string lastKnownResourceLocation;
        private bool activelySearching;

        public SearchForResource(AIBrain npcBrain) {
            this.npcBrain = npcBrain;
        }

        public void Tick() {
            tickTimer += Time.deltaTime;

            //Check for resources on timer to reduce CPU usage
            if (tickTimer > 0.5f && !activelySearching) {
                tickTimer = 0;
                // if we don't have a resource target, get one
                if (npcBrain.resourceTileTarget == null) {
                    GetTarget();
                } else if (npcBrain.resourceTileTarget.occupiedCategory == npcBrain.resourceCategoryWanted.ToString() 
                    && npcBrain.resourceTileTarget.occupiedStatus == ZetaUtilities.OCCUPIED_ITEMPICKUP
                    && npcBrain.resourceTileTarget.occupiedType == npcBrain.resourceTypeWanted.ToString() 
                    && npcBrain.pathAgent.isStopped && npcBrain.pathAgent.remainingDistance < 0.65) {

                    if (npcBrain.resourceTileTarget.GetTileObject().TryGetComponent(out DroppedResource resource)) {
                        if (resource.GetResourceData().GetResourceType().Equals(npcBrain.resourceCategoryWanted)) {
                            if (npcBrain.debugLogs) {
                                Debug.Log("SearchForResourceDrop.Tick(): Picking up dropped resource");
                            }

                            resource.PickUp();
                            npcBrain.resourceTileTarget = null;
                        }
                    } else {
                        Debug.LogWarning("SearchForResource.Tick(): Failed to get 'DroppedResource' component.");
                        npcBrain.resourceTileTarget = null;
                    }
                }
            } else if (activelySearching && npcBrain.pathAgent.isStopped) {
                activelySearching = false;
            }
        }

        public void OnEnter() {
            if (npcBrain.debugLogs) {
                Debug.Log("SearchForResource.OnEnter()");
            }
            tickTimer = 0;
            lastKnownResourceLocation = "last" + npcBrain.resourceCategoryWanted.ToString() + "_Node";
            npcBrain.resourceTileTarget = null;
            activelySearching = false;
        }

        public void OnExit() {
            
        }

        private void GetTarget() {
            if (npcBrain.debugLogs) {
                Debug.Log("Getting resource target");
            }

            // Get all of the tiles in the current and adjacent grids (3x3 chunk grid)
            List<WorldTile> fullTileGridList = new List<WorldTile>();
            MapManager.Instance.GetChunkGrid().GetXY(npcBrain.transform.position, out int chunkX, out int chunkY);

            for (int x = 0; x < 3; x++) {
                for (int y = 0; y < 3; y++) {
                    // check grid bounds
                    if (MapManager.Instance.GetChunkGrid().IsWithinGridBounds(chunkX + (x - 1), chunkY + (y - 1))) {
                        List<WorldTile> gridTileList = MapManager.Instance.GetChunkGrid().GetGridObject(chunkX + (x - 1), chunkY + (y - 1));
                        fullTileGridList.AddRange(gridTileList);
                    }
                }
            }

            WorldTile closestTile = null;

            // search for free dropped resources
            foreach (WorldTile tile in fullTileGridList) {
                if (tile.occupiedCategory == npcBrain.resourceCategoryWanted.ToString() && tile.occupiedStatus == ZetaUtilities.OCCUPIED_ITEMPICKUP && tile.occupiedType == npcBrain.resourceTypeWanted.ToString() && tile.HasTileObject()) {
                    if (closestTile == null) {
                        closestTile = tile;
                    } else if (Vector3.Distance(MapManager.Instance.GetWorldTileGrid().GetWorldPosition(tile.x, tile.y), npcBrain.transform.position) 
                        < Vector3.Distance(MapManager.Instance.GetWorldTileGrid().GetWorldPosition(closestTile.x, closestTile.y), npcBrain.transform.position) ) {
                        // If the distance between the targeted resource tile and the current position is less than the current closest tile and the current position, then make this the closest
                        closestTile = tile;
                    }
                }
            }

            // if a dropped resource is found, target the closest one found
            if (closestTile != null) {
                if (npcBrain.debugLogs) {
                    Debug.Log("SearchForResourceDrop.Tick(): dropped resource found");
                }

                activelySearching = true;
                npcBrain.resourceTileTarget = closestTile;
                npcBrain.pathAgent.destination = MapManager.Instance.GetWorldTileGrid().GetWorldPosition(closestTile.x, closestTile.y) + new Vector3(0.5f, 0.5f);
                npcBrain.pathAgent.SearchPath();
            } else {
                // otherwise, look for a resource node to harvest instead
                if (npcBrain.resourceTileTarget == null) {
                    foreach (WorldTile tile in fullTileGridList) {
                        if (tile.occupiedCategory == npcBrain.resourceCategoryWanted.ToString() && tile.occupiedStatus == ZetaUtilities.OCCUPIED_NODE_FULL && tile.occupiedType == npcBrain.resourceTypeWanted.ToString() && tile.HasTileObject()) {
                            if (closestTile == null) {
                                closestTile = tile;
                            } else if (Vector3.Distance(MapManager.Instance.GetWorldTileGrid().GetWorldPosition(tile.x, tile.y), npcBrain.transform.position)
                                < Vector3.Distance(MapManager.Instance.GetWorldTileGrid().GetWorldPosition(closestTile.x, closestTile.y), npcBrain.transform.position)) {
                                // If the distance between the targeted resource tile and the current position is less than the current closest tile and the current position, then make this the closest
                                closestTile = tile;
                            }
                        }
                    }
                }

                // if a resource node was found, target the closest one found
                if (closestTile != null) {
                    if (npcBrain.debugLogs) {
                        Debug.Log("SearchForResourceDrop.Tick(): resource node found");
                    }

                    npcBrain.npcMemory.AddMemory(lastKnownResourceLocation, MapManager.Instance.GetWorldTileGrid().GetWorldPosition(closestTile.x, closestTile.y));
                    npcBrain.resourceTileTarget = closestTile;
                } else {
                    // otherwise, travel to last known location of the resource if there is one remembered and not too far away
                    if (npcBrain.resourceTileTarget == null) {
                        if (npcBrain.npcMemory.ContainsMemory(lastKnownResourceLocation)) {
                            if (npcBrain.debugLogs) {
                                Debug.Log("SearchForResourceDrop.Tick(): memory found");
                            }

                            Vector3 memoryLocation = (Vector3)npcBrain.npcMemory.RetrieveMemory(lastKnownResourceLocation);

                            MapManager.Instance.GetWorldTileGrid().GetXY(npcBrain.transform.position, out int curX, out int curY);
                            MapManager.Instance.GetWorldTileGrid().GetXY(memoryLocation, out int memX, out int memY);

                            if (Mathf.Abs(memX - curX) < npcBrain.personality.maxDistanceFromPosition && Mathf.Abs(memY - curY) < npcBrain.personality.maxDistanceFromPosition) {
                                activelySearching = true;
                                npcBrain.pathAgent.destination = memoryLocation;
                                npcBrain.pathAgent.SearchPath();
                                npcBrain.npcMemory.RemoveMemory(lastKnownResourceLocation);
                            }
                        }
                    }
                }
            }
        }
    }
}

