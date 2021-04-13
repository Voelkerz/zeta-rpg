using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ZetaGames.RPG {
    public class AICollectWoodTask : MonoBehaviour {

        private Dictionary<ResourceType, int> inventory;
        //private ResourceManager resourceManager;
        private Collider2D destinationCollider;
        private Vector2 startingPosition;
        private int layerMask;
        private bool lookingForWood = true;
        private TaskManager taskManager;
        private NavMeshAgent navMeshAgent;
        public int amountOfWoodToCollect = 5;
        private FellTreeTask chopTask;

        void Start() {
            // GET TASK MANAGER
            taskManager = GetComponent<TaskManager>();

            // GET INVENTORY
            inventory = GetComponent<NpcInventory>().inventory;

            // GET NAV MESH AGENT
            navMeshAgent = gameObject.GetComponent<NavMeshAgent>();

            // FOR TESTING
            startingPosition = transform.position;
        }

        void Update() {
            if (lookingForWood && !taskManager.taskList.Contains(chopTask)) {
                lookForWood();
            } else {
                evaluateWoodStock();
            }
        }

        private void evaluateWoodStock() {
            if (!taskManager.taskList.Contains(chopTask)) {
                if (inventory[ResourceType.Wood] < amountOfWoodToCollect) {
                    lookingForWood = true;
                    Debug.Log("I'm looking for wood");
                }
            } else {
                lookingForWood = false;
            }
        }

        private void lookForWood() {
            // Do I have enough wood already?
            if (inventory[ResourceType.Wood] < amountOfWoodToCollect) {
                Debug.Log(name + ": I don't have enough wood");
                // Look for free wood on the ground first or a tree if no free wood is found
                lookForDroppedWood();
            } else {
                if (lookingForWood) {
                    Debug.Log(name + ": I have enough wood now");
                    lookingForWood = false;

                    NavMeshTask navTask = new NavMeshTask() {
                        taskID = 1,
                        priority = 1,
                        thisGameObject = gameObject,
                        agent = navMeshAgent,
                        destinationPosition = startingPosition
                    };

                    taskManager.taskList.Add(navTask);
                }
            }
        }

        private void lookForDroppedWood() {
            layerMask = 1 << 6; // layer mask 6 (dropped resources)
            destinationCollider = FindNearestCollider(ResourceType.Wood.ToString(), layerMask);
            if (destinationCollider != null) {
                Debug.Log(name + ": I found free wood!!!");
                lookingForWood = false;

                NavMeshTask navTask = new NavMeshTask() {
                    taskID = 1,
                    priority = 1,
                    thisGameObject = gameObject,
                    agent = navMeshAgent,
                    destinationPosition = destinationCollider.gameObject.transform.position
                };

                taskManager.taskList.Add(navTask);
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

                NavMeshTask navTask = new NavMeshTask() {
                    taskID = 1,
                    priority = 1,
                    thisGameObject = gameObject,
                    agent = navMeshAgent,
                    destinationPosition = destinationCollider.gameObject.transform.position + new Vector3(0, -1.25f)
                };

                taskManager.taskList.Add(navTask);

                Debug.Log(name + ": I found a tree!");

                taskManager.AddPause(1f);

                chopTask = new FellTreeTask() {
                    taskID = 3,
                    priority = 1,
                    charGameObject = gameObject,
                    treeGameObject = destinationCollider.gameObject
                };

                taskManager.taskList.Add(chopTask);
                taskManager.AddPause(1f);
                lookingForWood = false;
            } else {
                NavMeshTask navTask = new NavMeshTask() {
                    taskID = 1,
                    priority = 1,
                    thisGameObject = gameObject,
                    agent = navMeshAgent,
                    destinationPosition = startingPosition
                };

                taskManager.taskList.Add(navTask);
                
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
