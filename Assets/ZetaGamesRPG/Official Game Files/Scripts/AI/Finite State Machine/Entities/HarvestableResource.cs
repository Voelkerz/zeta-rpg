using UnityEngine;

namespace ZetaGames.RPG {
    public class HarvestableResource : MonoBehaviour {

        [SerializeField] private RecycleSubCategory recycleSubCategory;
        [SerializeField] private GameObject prefabResource;
        [SerializeField] private ResourceType resourceType;
        [SerializeField] private int hitPoints = 20;
        //private RecycleManager recycleManager;
        //private GameObject resourceToSpawn;

        private void Awake() {
            //recycleManager = FindObjectOfType<RecycleManager>();
        }

        public bool ToolHit(int hitAmount) {
            if (hitPoints > 0) {
                hitPoints -= hitAmount;
                return true;
            } else {
                return false;
            }
        }

        public ResourceType GetResourceType() {
            return resourceType;
        }

        public int GetHealth() {
            return hitPoints;
        }

        public void RecycleAndSpawnLoot() {
            // spawn resource loot from recycler
            //resourceToSpawn = recycleManager.GetRecycledObject(RecycleCategory.Resource, recycleSubCategory, prefabResource);
            //resourceToSpawn.transform.position = transform.position;
            //resourceToSpawn.SetActive(true);
            Instantiate(prefabResource, transform.position, Quaternion.identity);

            // recycle the resource source (this object)
            gameObject.SetActive(false);
            Destroy(gameObject, 0.75f);
            //recycleManager.RecycleObject(RecycleCategory.Resource, recycleSubCategory, gameObject);
        }
    }
}

