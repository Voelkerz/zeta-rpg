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

            hasBuildGoal = false;
            hasBuildSite = false;
            structureData = null;
            structCatGoal = StructureCategory.None;
            structTypeGoal = StructureType.None;
            structQualityGoal = EconomicClass.None;
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
            foreach (BaseStructureData data in BuildingManager.Instance.buildableStructures) {
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

        public void FindBuildSite() {
            Vector3 currentPos = npc.transform.position;
            ZetaGrid<WorldTile> mapGrid = MapManager.Instance.GetWorldTileGrid();
            int mapWidth = MapManager.Instance.mapWidth;
            int mapHeight = MapManager.Instance.mapHeight;
            bool siteTilesOccupied;

            for (int x = 0; x < 65; x++) {
                for (int y = 0; y < 65; y++) {
                    // check grid bounds
                    if ((int)currentPos.x + (x - 32) < mapWidth && (int)currentPos.y + (y - 32) < mapHeight && (int)currentPos.x + (x - 32) >= 0 && (int)currentPos.y + (y - 32) >= 0) {
                        WorldTile tile = mapGrid.GetGridObject((int)currentPos.x + (x - 32), (int)currentPos.y + (y - 32));
                        siteTilesOccupied = false;

                        if (!tile.occupied && tile.walkable) {
                            for (int siteX = 0; siteX < structureData.sizeX + 10; siteX++) {
                                for (int siteY = 0; siteY < structureData.sizeY + 10; siteY++) {
                                    if (siteX + tile.x < mapWidth && siteY + tile.y < mapHeight && siteX + tile.x >= 0 && siteY + tile.y >= 0) {
                                        WorldTile siteTile = mapGrid.GetGridObject(siteX + tile.x, siteY + tile.y);

                                        if (siteTile.occupied) {
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
                                buildSiteLocation = new Vector3(tile.x + 5, tile.y + 5);
                                return;
                            }
                        }
                    }
                }
            }

            /*
            // No suitable site found (walk around to find one)
            Vector3 destination = new Vector3(npc.transform.position.x + Random.Range(-30f, 30f), npc.transform.position.x + Random.Range(-30f, 30f));

            if (destination.x < mapWidth && destination.y < mapHeight && destination.x >= 0 && destination.y >= 0) {
                WorldTile destinationTile = mapGrid.GetGridObject((int)destination.x, (int)destination.y);
                if (destinationTile.walkable) {
                    npc.pathMovement.destination = destination;
                    npc.pathMovement.SearchPath();
                }
            }
            */
        }
    }
}
