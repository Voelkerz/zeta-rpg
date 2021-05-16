using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

namespace ZetaGames.RPG {
    public class BuildStructure : State {
        public override int priority => 10;
        public override bool isFinished => finished;
        public override bool isInterruptable => true;
        
        private bool finished;
        private readonly AIBrain npc;
        private BaseStructureData buildData;

        public BuildStructure(AIBrain npc) {
            this.npc = npc;
        }

        public override void OnEnter() {
            if (npc.debugLogs) {
                Debug.Log("BuildStructure.OnEnter(): Beginning to build");
            }

            if (npc.buildGoal.planned && npc.buildGoal.IsReadyToBuild()) {
                finished = false;
                buildData = npc.buildGoal.structureData;
                npc.pathMovement.destination = npc.buildGoal.buildSiteLocation + new Vector3(-1, -1);
                npc.pathMovement.SearchPath();
            } else {
                finished = true;
            }
        }

        public override void OnExit() {
            if (npc.debugLogs) {
                Debug.Log("BuildStructure.OnExit(): Finished building!");
            }
        }

        public override void Tick() {
            if (!finished) {
                if (npc.buildGoal.planned && npc.buildGoal.IsReadyToBuild()) {
                    if (npc.pathMovement.isStopped && Vector3.Distance(npc.transform.position, npc.buildGoal.buildSiteLocation) <= 2.5f) {
                        List<Vector3> buildingTileList = new List<Vector3>();
                        buildingTileList.AddRange(buildData.blockedTiles);
                        buildingTileList.AddRange(buildData.walkableTiles);
                        buildingTileList.AddRange(buildData.wallBoundary);
                        buildingTileList.Add(buildData.doorTile);

                        foreach (Vector3 tilePos in buildData.blockedTiles) {
                            WorldTile tile = MapManager.Instance.GetWorldTileGrid().GetGridObject(npc.buildGoal.buildSiteLocation + tilePos);
                            tile.occupied = true;
                            tile.occupiedStatus = ZetaUtilities.OCCUPIED_BUILDING;
                            tile.walkable = false;
                        }

                        foreach (Vector3 tilePos in buildData.wallBoundary) {
                            WorldTile tile = MapManager.Instance.GetWorldTileGrid().GetGridObject(npc.buildGoal.buildSiteLocation + tilePos);
                            tile.occupied = true;
                            tile.occupiedStatus = ZetaUtilities.OCCUPIED_BUILDING;
                            tile.walkable = false;
                        }

                        foreach (Vector3 tilePos in buildData.walkableTiles) {
                            WorldTile tile = MapManager.Instance.GetWorldTileGrid().GetGridObject(npc.buildGoal.buildSiteLocation + tilePos);
                            tile.occupied = true;
                            tile.occupiedStatus = ZetaUtilities.OCCUPIED_BUILDING;
                            tile.walkable = true;
                        }

                        // Door
                        WorldTile doorTile = MapManager.Instance.GetWorldTileGrid().GetGridObject(npc.buildGoal.buildSiteLocation + buildData.doorTile);
                        doorTile.occupied = true;
                        doorTile.occupiedStatus = ZetaUtilities.OCCUPIED_BUILDING;
                        doorTile.walkable = true;

                        UpdateAstarGraph();

                        npc.InstantiateObject(buildData.prefab, npc.buildGoal.buildSiteLocation);

                        npc.buildGoal.FinishBuildGoal();
                        finished = true;
                    }
                } else {
                    finished = true;
                }
            }
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
    }
}
