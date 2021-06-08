using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    public class NpcBuildGoal {

        private readonly AIBrain npc;
        private Dictionary<ResourceCategory, int> totalMaterialsRequired = new Dictionary<ResourceCategory, int>();
        private Dictionary<ResourceCategory, ResourceType> specificResourceTypesRequired = new Dictionary<ResourceCategory, ResourceType>();
        private StructureCategory structCatGoal = StructureCategory.None;
        private StructureType structTypeGoal = StructureType.None;
        private EconomicClass structQualityGoal = EconomicClass.None;
        public BaseStructureData structureData;
        public List<WorldTile> siteTiles = new List<WorldTile>();
        public Vector3 buildSiteLocation;
        public bool hasBuildSite;
        public bool hasBuildGoal;

        public NpcBuildGoal(AIBrain npc) {
            this.npc = npc;

            // Initialize Resource Dictionaries
            totalMaterialsRequired.Add(ResourceCategory.Wood, 0);
            totalMaterialsRequired.Add(ResourceCategory.Stone, 0);
            totalMaterialsRequired.Add(ResourceCategory.Ore, 0);
            totalMaterialsRequired.Add(ResourceCategory.Herb, 0);
            totalMaterialsRequired.Add(ResourceCategory.Gem, 0);
            totalMaterialsRequired.Add(ResourceCategory.Fiber, 0);

            specificResourceTypesRequired.Add(ResourceCategory.Wood, ResourceType.None);
            specificResourceTypesRequired.Add(ResourceCategory.Stone, ResourceType.None);
            specificResourceTypesRequired.Add(ResourceCategory.Ore, ResourceType.None);
            specificResourceTypesRequired.Add(ResourceCategory.Herb, ResourceType.None);
            specificResourceTypesRequired.Add(ResourceCategory.Gem, ResourceType.None);
            specificResourceTypesRequired.Add(ResourceCategory.Fiber, ResourceType.None);
        }

        public ResourceCategory GetRequiredMaterials() {
            foreach (ResourceCategory resourceCategory in totalMaterialsRequired.Keys) {
                if (totalMaterialsRequired[resourceCategory] > 0) {
                    return resourceCategory;
                }
            }

            return ResourceCategory.None;
        }

        public ResourceType GetRequiredResType(ResourceCategory resourceCategory) {
            return specificResourceTypesRequired[resourceCategory];
        }

        public int GetResourceAmount(ResourceCategory category) {
            return totalMaterialsRequired[category];
        }

        public bool HasRequiredMaterialsInInventory() {
            if (hasBuildGoal) {
                foreach (ResourceCategory resource in totalMaterialsRequired.Keys) {
                    if (totalMaterialsRequired[resource] > 0) {
                        if (npc.inventory.GetAmountOfResource(resource) > 0) {
                            return true;
                        }
                    }
                }
                return false;
            } else {
                return false;
            }
        }

        public bool HasAllMaterials() {
            if (hasBuildGoal) {
                foreach (int numMaterialNeeded in totalMaterialsRequired.Values) {
                    if (numMaterialNeeded > 0) {
                        return false;
                    }
                }
                return true;
            } else {
                return false;
            }
        }

        public void FinishBuildGoal() {
            if (structCatGoal.Equals(StructureCategory.Home)) {
                npc.stats.homeProperty = structureData;
                npc.memory.AddMemory("Home", buildSiteLocation);
            }

            ResetBuildGoal();
        }

        public void CreateBuildGoal(StructureCategory category, StructureType type, EconomicClass quality) {
            hasBuildGoal = true;
            hasBuildSite = false;
            structCatGoal = category;
            structTypeGoal = type;
            structQualityGoal = quality;
            structureData = GetStructureData();

            CalculateResourcesNeeded();
            FindBuildSite();
        }

        public void ResetBuildGoal() {
            hasBuildGoal = false;
            hasBuildSite = false;
            structureData = null;
            structCatGoal = StructureCategory.None;
            structTypeGoal = StructureType.None;
            structQualityGoal = EconomicClass.None;

            totalMaterialsRequired[ResourceCategory.Wood] = 0;
            totalMaterialsRequired[ResourceCategory.Stone] = 0;
            totalMaterialsRequired[ResourceCategory.Ore] = 0;
            totalMaterialsRequired[ResourceCategory.Herb] = 0;
            totalMaterialsRequired[ResourceCategory.Gem] = 0;
            totalMaterialsRequired[ResourceCategory.Fiber] = 0;
        }

        public void AlterResourceAmount(ResourceCategory resourceCategory, int amount) {
            totalMaterialsRequired[resourceCategory] += amount;

            if (totalMaterialsRequired[resourceCategory] < 0) {
                totalMaterialsRequired[resourceCategory] = 0;
            }

            if (npc.debugLogs) {
                if (npc.buildGoal.GetResourceAmount(resourceCategory) == 0) {
                    Debug.Log("Collected all the " + resourceCategory.ToString() + " that I needed.");
                }
            }
        }

        private BaseStructureData GetStructureData() {
            foreach (BaseStructureData data in TilemapObstacleManager.Instance.buildableStructures) {
                if (data.category.Equals(structCatGoal)
                    && data.type.Equals(structTypeGoal)
                    && data.quality.Equals(structQualityGoal)) {

                    return data;
                }
            }
            return null;
        }

        private void CalculateResourcesNeeded() {
            // Building Material 1
            if (!structureData.material1.Equals(ResourceCategory.None)) {
                totalMaterialsRequired[structureData.material1] = structureData.material1Amount - npc.inventory.GetAmountOfResource(structureData.material1);

                if (totalMaterialsRequired[structureData.material1] < 0) {
                    totalMaterialsRequired[structureData.material1] = 0;
                }
            }

            // Building Material 2
            if (!structureData.material2.Equals(ResourceCategory.None)) {
                totalMaterialsRequired[structureData.material2] = structureData.material2Amount - npc.inventory.GetAmountOfResource(structureData.material2);

                if (totalMaterialsRequired[structureData.material2] < 0) {
                    totalMaterialsRequired[structureData.material2] = 0;
                }
            }
        }

        private bool CalculateSiteLocation(WorldTile[] worldTiles) {
            ZetaGrid<WorldTile> mapGrid = MapManager.Instance.GetWorldTileGrid();
            int mapWidth = MapManager.Instance.mapWidth;
            int mapHeight = MapManager.Instance.mapHeight;
            bool siteTilesOccupied;
            int randomBuffer = Random.Range(2, 7);

            foreach (WorldTile settlementTiles in worldTiles) {
                siteTilesOccupied = false;
                siteTiles.Clear();

                if (!settlementTiles.occupied && settlementTiles.walkable && !settlementTiles.terrainType.Equals(ZetaUtilities.TERRAIN_SETTLEMENT_ROAD)) {
                    for (int siteX = 0; siteX < structureData.sizeX + randomBuffer; siteX++) {
                        for (int siteY = 0; siteY < structureData.sizeY + randomBuffer; siteY++) {
                            if (siteX + settlementTiles.x < mapWidth && siteY + settlementTiles.y < mapHeight && siteX + settlementTiles.x >= 0 && siteY + settlementTiles.y >= 0) {
                                WorldTile siteTile = mapGrid.GetGridObject(siteX + settlementTiles.x, siteY + settlementTiles.y);
                                siteTiles.Add(siteTile);

                                if (siteTile.occupied || siteTile.lockTag != -1 || siteTile.terrainType.Equals(ZetaUtilities.TERRAIN_SETTLEMENT_ROAD)) {
                                    siteTilesOccupied = true;
                                    break;
                                }
                            }
                        }

                        if (siteTilesOccupied) {
                            break;
                        }
                    }

                    // site for building found
                    if (!siteTilesOccupied) {
                        buildSiteLocation = new Vector3(settlementTiles.x + (randomBuffer / 2), settlementTiles.y + (randomBuffer / 2));

                        // Lock this site to this NPC so no others build there
                        siteTiles.Add(settlementTiles);

                        foreach (WorldTile sTile in siteTiles) {
                            sTile.lockTag = npc.lockTag;
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        public void FindBuildSite() {
            if (npc.stats.settlement != null) {
                bool foundBuildSite;
                int randIndex;

                //***************************//
                // Check origin region first
                //***************************//
                WorldTile[] originRegionTiles = MapManager.Instance.GetWorldRegionGrid().GetGridObject(npc.stats.settlement.originRegion.x, npc.stats.settlement.originRegion.y);
                foundBuildSite = CalculateSiteLocation(originRegionTiles);
                if (foundBuildSite) return;

                //**************************************************//
                // Check random settled regions in this settlement
                //**************************************************//
                List<Settlement.Region> settledRegions = new List<Settlement.Region>();

                foreach (Settlement.Region region in npc.stats.settlement.growthRegions) {
                    if (region.settled) {
                        settledRegions.Add(region);
                    }
                }

                if (settledRegions.Count != 0) {
                    randIndex = Random.Range(0, settledRegions.Count);
                    WorldTile[] settledRegionTiles = MapManager.Instance.GetWorldRegionGrid().GetGridObject(settledRegions[randIndex].x, settledRegions[randIndex].y);
                    foundBuildSite = CalculateSiteLocation(settledRegionTiles);
                    if (foundBuildSite) return;
                } else {
                    Debug.LogWarning("No settled regions added");
                }

                //******************************************************************//
                // Check empty regions immediately bordering a settlement entrance
                //******************************************************************//
                List<Settlement.Region> unsettledRegions = new List<Settlement.Region>();

                foreach (Settlement.Region region in npc.stats.settlement.growthRegions) {
                    if (!region.settled && (region.eastExit || region.westExit || region.northExit || region.southExit)) {
                        unsettledRegions.Add(region);
                    }
                }

                if (unsettledRegions.Count != 0) {
                    randIndex = Random.Range(0, unsettledRegions.Count);
                    WorldTile[] unsettledRegionTiles = MapManager.Instance.GetWorldRegionGrid().GetGridObject(unsettledRegions[randIndex].x, unsettledRegions[randIndex].y);
                    foundBuildSite = CalculateSiteLocation(unsettledRegionTiles);
                    if (foundBuildSite) return;
                } else {
                    Debug.LogWarning("No unsettled regions added");
                }


                //**********************************************************//
                // No suitable site found at settlement. Leave settlement.
                //**********************************************************//
                CommunityManager.Instance.LeaveSettlement(npc.stats.settlement, npc.gameObject);
                npc.stats.settlement.atMax = true;
                npc.stats.settlement = null;
                npc.joinCommunity.hasCommunity = false;

                // Reset build goal
                ResetBuildGoal();
            }
        }
    }
}
