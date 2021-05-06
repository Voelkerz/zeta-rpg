using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    

    public class NpcManager : MonoBehaviour {
        // A master list of NPCs that includes their unique ID and a list of resouces in their inventory
        private Dictionary<int, Dictionary<ResourceCategory, int>> npcList = new Dictionary<int, Dictionary<ResourceCategory, int>>();
        private Dictionary<ResourceCategory, int> inventory;
        private GameObject[] initialNpcList;


        void Awake() {

        }

        public void addNPC(int id) {
            if (!npcList.ContainsKey(id)) {
                npcList.Add(id, initializeNpcInventory());
            }
        }
        
        public void addResource(int id, ResourceCategory type, int amount) {
            npcList[id][type] += amount;
        }

        public void subtractResource(int id, ResourceCategory type, int amount) {
            npcList[id][type] -= amount;
        }

        public Dictionary<ResourceCategory, int> getNpcInventory(int id) {
            if (npcList.ContainsKey(id)) {
                return npcList[id];
            } else { return null; }
        }

        public Dictionary<ResourceCategory, int> initializeNpcInventory() {
            inventory = new Dictionary<ResourceCategory, int>();
            inventory.Add(ResourceCategory.Coin, 100);
            inventory.Add(ResourceCategory.Wood, 0);
            inventory.Add(ResourceCategory.Stone, 0);
            inventory.Add(ResourceCategory.Food, 0);
            return inventory;
        }
    }
}
