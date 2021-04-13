using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    public class NpcInventory : MonoBehaviour {

        private ResourceManager resourceManager;
        public Dictionary<ResourceType, int> inventory {get; set;}
        public int maxInventoryCapacity = 10;
        private float timerMax = 60f;
        private float timer;

        // EDITOR PROPERTIES
        public int numWood = 0;

        void Awake() {
            // INVENTORY SETUP
            resourceManager = FindObjectOfType<ResourceManager>();
            resourceManager.addNPC(gameObject.GetInstanceID());
            inventory = resourceManager.getNpcInventory(gameObject.GetInstanceID());
            timer = timerMax;
        }

        void Update() {
            if (timer <= 0) {
                timer = timerMax;
                if (inventory.TryGetValue(ResourceType.Wood, out int numWood)) {
                    this.numWood = numWood;
                }
            } else {
                timer--;
            }
        }
    }
}
