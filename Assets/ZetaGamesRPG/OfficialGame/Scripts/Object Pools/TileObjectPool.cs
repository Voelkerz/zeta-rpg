using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    public class TileObjectPool : MonoBehaviour {

        public static TileObjectPool SharedInstance;
        public Dictionary<string, List<GameObject>> pooledObjects;
        public List<GameObject> objectsToPool;
        public int startingAmount;
        private int additionalAmount;
        public bool flexibleAmount;

        private void Awake() {
            SharedInstance = this;
        }

        void Start() {
            pooledObjects = new Dictionary<string, List<GameObject>>();
            GameObject tmp;

            foreach (GameObject prefab in objectsToPool) {
                List<GameObject> categoryList = new List<GameObject>();
                
                // for each object type to pool, create a starting list
                for (int i = 0; i < startingAmount; i++) {
                    tmp = Instantiate(prefab, gameObject.transform);
                    tmp.tag = ZetaUtilities.TAG_CULLED;
                    categoryList.Add(tmp);
                }

                // add to pool dictionary
                pooledObjects.Add(prefab.name, categoryList);
            }
        }

        public GameObject GetPooledObject(string prefabName) {
            for (int i = 0; i < pooledObjects[prefabName].Count; i++) {
                if (pooledObjects[prefabName][i].tag == ZetaUtilities.TAG_CULLED) {
                    return pooledObjects[prefabName][i];
                }
            }

            if (flexibleAmount) {
                foreach (GameObject prefab in objectsToPool) {
                    if (prefab.name == prefabName) {
                        GameObject newPooledObject = Instantiate(prefab, gameObject.transform);
                        newPooledObject.tag = ZetaUtilities.TAG_CULLED;
                        pooledObjects[prefabName].Add(newPooledObject);
                        additionalAmount++;

                        //Debug.Log("Created an additional pooled object to use.");

                        return newPooledObject;
                    } 
                }

                return null;
            } else {
                return null;
            }
        }
    }
}
