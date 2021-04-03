using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ZetaGames.RPG {
    public enum ResourceType {
        Wood,
        Stone,
        Coin,
        Food
    }

    public class ResourceManager : MonoBehaviour {
        // A master list of NPCs that includes their unique ID and a list of resouces in their inventory
        private Dictionary<int, Dictionary<ResourceType, int>> npcList = new Dictionary<int, Dictionary<ResourceType, int>>();
        private Dictionary<ResourceType, int> inventory;
        private GameObject[] initialNpcList;


        void Start() {
            // Initialize on character instantiation instead
            /*
            // find all friendly NPCs in scene and initialize their inventories
            initialNpcList = GameObject.FindGameObjectsWithTag("Friendly");
            foreach (GameObject npc in initialNpcList) {
                npcList.Add(npc.GetInstanceID(), initializeNpcInventory());
            }
            */
        }

        public void addNPC(int id) {
            if (!npcList.ContainsKey(id)) {
                npcList.Add(id, initializeNpcInventory());
            }
        }
        
        public void addResource(int id, ResourceType type, int amount) {
            npcList[id][type] += amount;
        }

        public void subtractResource(int id, ResourceType type, int amount) {
            npcList[id][type] -= amount;
        }

        public Dictionary<ResourceType, int> getNpcInventory(int id) {
            if (npcList.ContainsKey(id)) {
                return npcList[id];
            } else { return null; }
        }

        public Dictionary<ResourceType, int> initializeNpcInventory() {
            inventory = new Dictionary<ResourceType, int>();
            inventory.Add(ResourceType.Coin, 100);
            inventory.Add(ResourceType.Wood, 0);
            inventory.Add(ResourceType.Stone, 0);
            inventory.Add(ResourceType.Food, 0);
            return inventory;
        }
    }
}
