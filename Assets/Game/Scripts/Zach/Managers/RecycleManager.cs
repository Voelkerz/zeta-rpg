using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * TODO:
 * This needs some work done.
 * Problem is that I can't find a reliable
 * way of comparing gameObjects to each other
 * so that it instantiates the correct one.
*/

namespace ZetaGames.RPG {
    public enum RecycleCategory {
        Resource,
        Item,
        NPC,
        Enemy
    }

    public enum RecycleSubCategory {
        // RESOURCE
        Wood,
        Stone,
        Food,
        Coin
    }

    public class RecycleManager : MonoBehaviour {
        // MAIN DICTIONARY
        private Dictionary<RecycleCategory, Dictionary<RecycleSubCategory, HashSet<GameObject>>> recycleDictionary = new Dictionary<RecycleCategory, Dictionary<RecycleSubCategory, HashSet<GameObject>>>();

        // TOP LEVEL CATEGORIES
        private Dictionary<RecycleSubCategory, HashSet<GameObject>> resourceObjects = new Dictionary<RecycleSubCategory, HashSet<GameObject>>();
        private Dictionary<RecycleSubCategory, HashSet<GameObject>> itemObjects = new Dictionary<RecycleSubCategory, HashSet<GameObject>>();
        private Dictionary<RecycleSubCategory, HashSet<GameObject>> npcObjects = new Dictionary<RecycleSubCategory, HashSet<GameObject>>();
        private Dictionary<RecycleSubCategory, HashSet<GameObject>> enemyObjects = new Dictionary<RecycleSubCategory, HashSet<GameObject>>();

        // RESOURCE SUB CATEGORIES


        void Start() {
            // ADD CATEGORIES TO DICTIONARY
            recycleDictionary.Add(RecycleCategory.Resource, resourceObjects);
            recycleDictionary.Add(RecycleCategory.Item, itemObjects);
            recycleDictionary.Add(RecycleCategory.NPC, npcObjects);
            recycleDictionary.Add(RecycleCategory.Enemy, enemyObjects);
        }
       
        public GameObject GetRecycledObject(RecycleCategory category, RecycleSubCategory subCategory, GameObject objectToMatch) {
            int count = 0;

            // create sub category dictionary if it doesn't exist
            if (!recycleDictionary[category].ContainsKey(subCategory)) {
                recycleDictionary[category].Add(subCategory, new HashSet<GameObject>());
                Debug.Log("recycled subcategory '" + subCategory.ToString() + "' did not exist. Initializing new subcategory with empty list of objects.");
            }
            
            foreach (GameObject recycledObject in recycleDictionary[category][subCategory]) {
                count++;
                Debug.Log("Recycled Count: " + count);
                if (objectToMatch) {
                    return recycledObject;
                }
            }

            // if no matching game objects were in the recycler, then instantiate a brand new one
            return Instantiate(objectToMatch);
        }

        public void RecycleObject(RecycleCategory category, RecycleSubCategory subCategory, GameObject objectToRecycle) {
            // create sub category dictionary if it doesn't exist
            if (!recycleDictionary[category].ContainsKey(subCategory)) {
                recycleDictionary[category].Add(subCategory, new HashSet<GameObject>());
                recycleDictionary[category][subCategory].Add(objectToRecycle);
                Debug.Log("recycled subcategory '" + subCategory.ToString() + "' did not exist. Initializing new subcategory with first recycled object.");
            } else {
                recycleDictionary[category][subCategory].Add(objectToRecycle);
            }
        }
    }
}

