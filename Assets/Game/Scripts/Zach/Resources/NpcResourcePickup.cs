using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    public class NpcResourcePickup : MonoBehaviour {
        public ResourceType resourceType;
        private ResourceManager resourceManager;

        //debug var
        private Dictionary<ResourceType, int> resourceList;

        private void OnTriggerEnter2D(Collider2D collision) {
            if (collision.tag == "Friendly" || collision.tag == "Enemy") {
                resourceManager = FindObjectOfType<ResourceManager>();
                resourceManager.addResource(collision.gameObject.GetInstanceID(), resourceType, 1);

                //debug
                /*
                resourceList = resourceManager.getNpcInventory(collision.gameObject.GetInstanceID());
                Debug.Log(collision.name + "'s Resources:");
                printInventory();
                */
                //

                Destroy(gameObject);
            }
        }

        // debug method
        private void printInventory() {
            Debug.Log("==========================");
            foreach (KeyValuePair<ResourceType, int> resourceInfo in resourceList) {
                Debug.Log(resourceInfo.Key + ": " + resourceInfo.Value);
            }
            Debug.Log("==========================");
        }
    }
}
