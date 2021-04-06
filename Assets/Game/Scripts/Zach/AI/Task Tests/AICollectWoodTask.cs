using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    public class AICollectWoodTask : MonoBehaviour {

        private Dictionary<ResourceType, int> inventory;
        //private ResourceManager resourceManager;
        private Collider2D destinationCollider;
        private Vector2 startingPosition;
        private int layerMask;
        private bool lookingForWood = true;
        private TaskManager taskManager;
        public int amountOfWoodToCollect = 5;

        private void Start() {
            // GET TASK MANAGER
            taskManager = GetComponent<TaskManager>();

            // GET INVENTORY
            inventory = GetComponent<InventoryManager>().inventory;

            // FOR TESTING
            startingPosition = transform.position;
        }

        private void Update() {
            if (taskManager.taskList.Count == 0 && lookingForWood) {
                lookForWood();
            }
        }

        private void lookForWood() {
            // Am I looking for wood?
            if (lookingForWood) {
                // Do I have enough wood already?
                if (inventory[ResourceType.Wood] < amountOfWoodToCollect) {
                    Debug.Log(name + ": I don't have enough wood");
                    // Look for free wood on the ground first or a tree if no free wood is found
                    lookForDroppedWood();
                } else {
                    if (lookingForWood) {
                        Debug.Log(name + ": I have enough wood now");
                        lookingForWood = false;
                        taskManager.AddNavMeshAgentMoveTask(startingPosition);
                    }
                }
            }
        }

        private void lookForDroppedWood() {
            layerMask = 1 << 6; // layer mask 6 (dropped resources)
            destinationCollider = FindNearestCollider(ResourceType.Wood.ToString(), layerMask);
            if (destinationCollider != null) {
                taskManager.AddNavMeshAgentMoveTask(destinationCollider.gameObject.transform.position);
                Debug.Log(name + ": I found free wood!!!");
            } else {
                Debug.Log(name + ": I couldn't find any free wood. I'll look for a tree instead");
                lookForTree();
            }
        }

        private void lookForTree() {
            // Find nearest tree
            layerMask = 1 << 7; // layer mask 7 (resource sources)
            destinationCollider = FindNearestCollider(ResourceType.Wood.ToString(), layerMask);
            // If tree is found
            if (destinationCollider != null) {
                taskManager.AddNavMeshAgentMoveTask(destinationCollider.gameObject.transform.position + new Vector3(0, -1.25f));
                
                Debug.Log(name + ": I found a tree!");

                FellTreeTask task = new FellTreeTask() {
                    taskID = 1,
                    priority = 1,
                    charGameObject = gameObject,
                    treeGameObject = destinationCollider.gameObject
                };

                taskManager.taskList.Add(task);
            } else {
                taskManager.AddNavMeshAgentMoveTask(startingPosition);
                Debug.Log(name + ": I couldn't find any trees");
                lookingForWood = false;
            }
        }

        private Collider2D FindNearestCollider(string tag, int layerMask) {             
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 100f, layerMask);
            Collider2D nearestCollider = null;

            float minSqrDistance = Mathf.Infinity;

            for (int i = 0; i < colliders.Length; i++) {
                if (colliders[i].tag == tag) {
                    float sqrDistanceToCenter = (transform.position - colliders[i].transform.position).sqrMagnitude;
                    if (sqrDistanceToCenter < minSqrDistance) {
                        minSqrDistance = sqrDistanceToCenter;
                        nearestCollider = colliders[i];
                    }
                }
            }

            return nearestCollider;
        }
    }
}
