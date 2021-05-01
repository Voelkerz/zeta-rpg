using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    public abstract class ObjectPool : MonoBehaviour {

        public List<GameObject> pooledObjects;
        public GameObject objectToPool;
        public int startingAmount;
        private int additionalAmount;
        public bool flexibleAmount;

        void Start() {
            pooledObjects = new List<GameObject>();
            GameObject tmp;

            for (int i = 0; i < startingAmount; i++) {
                tmp = Instantiate(objectToPool, gameObject.transform);
                //tmp.SetActive(false);
                tmp.tag = "Culled";
                pooledObjects.Add(tmp);
            }
        }

        public GameObject GetPooledObject() {
            for (int i = 0; i < startingAmount + additionalAmount; i++) {
                if (pooledObjects[i].tag == "Culled") {
                    return pooledObjects[i];
                }
            }

            if (flexibleAmount) {
                GameObject newPooledObject = Instantiate(objectToPool, gameObject.transform);
                //newPooledObject.SetActive(false);
                pooledObjects.Add(newPooledObject);
                additionalAmount++;

                Debug.Log("Created an additional pooled object to use. Total is: " + (startingAmount + additionalAmount));

                return newPooledObject;
            } else {
                return null;
            }
        }
    }
}
