using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    public enum ResourceType {
        Wood,
        Stone,
        Coin,
        Food
    }

    public class ResourceManager : MonoBehaviour {
        // the full list of NPCs and their personal amount of resources
        public Dictionary<int, Dictionary<ResourceType, int>> npcList = new Dictionary<int, Dictionary<ResourceType, int>>();

        // the list that will hold the amount of resources an NPC has
        private Dictionary<ResourceType, int> resourceList;

        private GameObject[] initialNpcList;

        void Start() {
            // find all friendly NPCs in scene and initialize their inventories
            initialNpcList = GameObject.FindGameObjectsWithTag("Friendly");
            foreach (GameObject npc in initialNpcList) {
                npcList.Add(npc.GetInstanceID(), initializeNpcResources());
            }
        }

        public void addNPC(int id, Dictionary<ResourceType, int> resources) {
            if (!npcList.ContainsKey(id)) {
                npcList.Add(id, resources);
            }
        }
        
        public void addResource(int id, ResourceType type, int amount) {
            npcList[id][type] += amount;
        }

        public void subtractResource(int id, ResourceType type, int amount) {
            npcList[id][type] -= amount;
        }

        public Dictionary<ResourceType, int> getNpcResources(int id) {
            if (npcList.ContainsKey(id)) {
                return npcList[id];
            } else { return null; }
        }

        public Dictionary<ResourceType, int> initializeNpcResources() {
            resourceList = new Dictionary<ResourceType, int>();
            resourceList.Add(ResourceType.Coin, 0);
            resourceList.Add(ResourceType.Wood, 0);
            resourceList.Add(ResourceType.Stone, 0);
            resourceList.Add(ResourceType.Food, 0);
            return resourceList;
        }
    }
}
