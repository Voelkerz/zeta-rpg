using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    public class DroppedResource : MonoBehaviour {
        [SerializeField] private RecycleSubCategory recycleSubCategory;
        [SerializeField] private ResourceData resourcecData;
        //private RecycleManager recycleManager;
        private NpcInventory npcInventory;

        private void Awake() {
            //recycleManager = FindObjectOfType<RecycleManager>();
        }

        private void OnTriggerEnter2D(Collider2D collider) {
            if (collider.tag == "NPC") {
                npcInventory = collider.GetComponent<NpcInventory>();
            }
        }

        public bool PickUp() {
            if (npcInventory != null) {
                npcInventory.PickupResource(resourcecData.GetResourceType());
                // recycle the object
                gameObject.SetActive(false);
                Destroy(gameObject, 0.75f);
                //recycleManager.RecycleObject(RecycleCategory.Resource, recycleSubCategory, gameObject);
                return true;
            } else {
                Debug.LogWarning("DroppedResource.PickUp(): npcInventory null, collider may be improperly set.");
                return false;
            }
            
        }
    }
}
