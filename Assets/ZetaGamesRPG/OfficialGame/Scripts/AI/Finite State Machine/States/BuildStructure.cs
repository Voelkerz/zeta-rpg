using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

namespace ZetaGames.RPG {
    public class BuildStructure : State {
        public override bool isFinished => finished;
        public override bool isInterruptable { get => npc.inCombat; }
        public override float actionScore { get => 10; set => actionScore = value; }

        private bool finished;
        private readonly AIBrain npc;
        private BaseStructureData buildData;

        public BuildStructure(AIBrain npc) {
            this.npc = npc;
        }

        public override void OnEnter() {
            finished = false;
            buildData = npc.buildGoal.structureData;
            npc.pathMovement.destination = npc.buildGoal.buildSiteLocation + new Vector3(-1, -1);
            npc.pathMovement.SearchPath();

            if (npc.debugLogs) {
                Debug.Log("BuildStructure.OnEnter(): Entering build mode.");
            }
        }

        public override void OnExit() {
            if (finished) buildData = null;

            if (npc.debugLogs) {
                Debug.Log("BuildStructure.OnExit(): Exiting build mode.");
            }
        }

        public override void Tick() {
            if (!finished) {
                if (npc.pathMovement.isStopped && Vector3.Distance(npc.transform.position, npc.buildGoal.buildSiteLocation) <= 3f) {
                    if (!npc.buildGoal.hasBuildSite) {
                        // Place the build site
                        npc.buildGoal.hasBuildSite = PlaceBuildSite();
                    } else if (npc.buildGoal.HasRequiredMaterialsInInventory() && npc.buildGoal.hasBuildSite) {
                        // Use build tool to progress the construction (better tools will mean faster construction)
                        ConstructionAction();

                        // Check to see if all materials have been supplied to build site
                        if (npc.buildGoal.HasAllMaterials()) {
                            InstantiateStructure();
                            npc.buildGoal.FinishBuildGoal();
                            finished = true;
                        }
                    } else {
                        finished = true;
                    }
                } else {
                    if (npc.debugLogs) {
                        Debug.Log("BuildStructure.Tick(): Moving towards build site.");
                    }
                }
            }
        }

        private void ConstructionAction() {
            if (npc.useAdvAI) {
                //TODO: Put in building npc animations here
                // or anything you want to see on screen.
            }

            // Place construction materials in buildsite with tool
            if (!ToolHit(1)) {
                if (npc.debugLogs) {
                    Debug.LogError("Failed to do construction action");
                }
            }
        }

        public bool ToolHit(int materials) {
            ResourceCategory resource = ResourceCategory.None;

            if (npc.inventory.GetAmountOfResource(buildData.material1) > 0) {
                resource = buildData.material1;
            } else if (npc.inventory.GetAmountOfResource(buildData.material2) > 0) {
                resource = buildData.material2;
            }

            if (!resource.Equals(ResourceCategory.None)) {
                // Remove from NPC held inventory
                int leftovers = npc.inventory.RemoveResource(resource, materials);

                // If there are leftovers, it means the tool tried to use more resources than what the NPC had.
                // Change to hit with the amount of resources the NPC did have in its inventory.
                if (leftovers > 0) {
                    materials -= leftovers;
                }

                // Remove from tracked build goal resource list
                npc.buildGoal.AlterResourceAmount(resource, -materials);

                // Evaluate inventory fullness
                npc.inventory.needToStoreItems = npc.inventory.IsInventoryFullOfResource(resource);
                
                //Debug.Log("Moved " + materials + " material(s). BuildGoal at: " + npc.buildGoal.GetResourceAmount(resource) + " || Inventory at: " + npc.inventory.GetAmountOfResource(resource));
                return true;
            } else {
                return false;
            }
            
        }

        private void InstantiateStructure() {
            List<Vector3> buildingTileList = new List<Vector3>();
            buildingTileList.AddRange(buildData.blockedTiles);
            buildingTileList.AddRange(buildData.walkableTiles);
            buildingTileList.AddRange(buildData.wallBoundary);
            buildingTileList.Add(buildData.doorTile);

            foreach (Vector3 tilePos in buildData.blockedTiles) {
                WorldTile tile = MapManager.Instance.GetWorldTileGrid().GetGridObject(npc.buildGoal.buildSiteLocation + tilePos);
                tile.occupied = true;
                tile.occupiedStatus = ZetaUtilities.OCCUPIED_STRUCTURE;
                tile.walkable = false;
            }

            foreach (Vector3 tilePos in buildData.wallBoundary) {
                WorldTile tile = MapManager.Instance.GetWorldTileGrid().GetGridObject(npc.buildGoal.buildSiteLocation + tilePos);
                tile.occupied = true;
                tile.occupiedStatus = ZetaUtilities.OCCUPIED_STRUCTURE_WALL;
                tile.walkable = false;
            }

            foreach (Vector3 tilePos in buildData.walkableTiles) {
                WorldTile tile = MapManager.Instance.GetWorldTileGrid().GetGridObject(npc.buildGoal.buildSiteLocation + tilePos);
                tile.occupied = true;
                tile.occupiedStatus = ZetaUtilities.OCCUPIED_STRUCTURE;
                tile.walkable = true;
            }

            // Door
            WorldTile doorTile = MapManager.Instance.GetWorldTileGrid().GetGridObject(npc.buildGoal.buildSiteLocation + buildData.doorTile);
            doorTile.occupied = true;
            doorTile.occupiedStatus = ZetaUtilities.OCCUPIED_STRUCTURE_DOOR;
            doorTile.walkable = true;

            UpdateAstarGraph();

            npc.InstantiateObject(buildData.prefab, npc.buildGoal.buildSiteLocation);
        }

        private void UpdateAstarGraph() {
            AstarPath.active.AddWorkItem(new AstarWorkItem(ctx => {
                GridGraph gg = AstarPath.active.data.gridGraph;

                for (int y = 0; y < buildData.sizeY; y++) {
                    for (int x = 0; x < buildData.sizeX; x++) {
                        GridNodeBase node = gg.GetNode((int)npc.buildGoal.buildSiteLocation.x + x, (int)npc.buildGoal.buildSiteLocation.y + y);
                        WorldTile worldTile = MapManager.Instance.GetWorldTileGrid().GetGridObject(new Vector3((int)npc.buildGoal.buildSiteLocation.x + x, (int)npc.buildGoal.buildSiteLocation.y + y));

                        node.Walkable = worldTile.walkable;
                    }
                }

                gg.GetNodes(node => gg.CalculateConnections((GridNodeBase)node));
            }));
        }

        private bool PlaceBuildSite() {
            // Final check for build site validity
            foreach (WorldTile tile in npc.buildGoal.siteTiles) {
                if (tile.occupied || !tile.walkable || tile.lockTag != npc.lockTag) {
                    npc.buildGoal.ResetBuildGoal();
                    finished = true;
                    return false;
                }
            }

            if (!MapManager.Instance.GetWorldTileGrid().IsWithinGridBounds((int)npc.buildGoal.buildSiteLocation.x, (int)npc.buildGoal.buildSiteLocation.y) || buildData == null) {
                npc.buildGoal.ResetBuildGoal();
                finished = true;
                return false;
            }

            List<WorldTile> updatedTiles = new List<WorldTile>();
            List<Vector3> buildingTileList = new List<Vector3>();
            buildingTileList.AddRange(buildData.blockedTiles);
            buildingTileList.AddRange(buildData.walkableTiles);
            buildingTileList.AddRange(buildData.wallBoundary);
            buildingTileList.Add(buildData.doorTile);

            foreach (Vector3 tilePos in buildingTileList) {
                WorldTile tile = MapManager.Instance.GetWorldTileGrid().GetGridObject(npc.buildGoal.buildSiteLocation + tilePos);

                if (tile == null) {
                    npc.buildGoal.ResetBuildGoal();
                    finished = true;
                    return false;
                }

                // Set dirt ground as base
                MapManager.Instance.SetMapSpriteTile(tile, ZetaUtilities.TILEMAP_BASE + tile.elevation, "Minifantasy_TownsDirt", false);

                // Nullify map decor layer
                MapManager.Instance.tileMapList[4].SetTile(tile.GetWorldPositionInt(), null);
                MapManager.Instance.tileMapList[5].SetTile(tile.GetWorldPositionInt(), null);

                // Update tile data
                tile.occupied = true;
                tile.occupiedStatus = ZetaUtilities.OCCUPIED_STRUCTURE_BUILDSITE;
                tile.walkable = false;

                updatedTiles.Add(tile);
            }

            ZetaUtilities.UpdateMultipleAstarGraphNodes(updatedTiles);

            return true;
        }

        public override float GetUtilityScore() {
            throw new System.NotImplementedException();
        }

        public override void AddUtilityScore(float amount) {
            throw new System.NotImplementedException();
        }
    }
}
