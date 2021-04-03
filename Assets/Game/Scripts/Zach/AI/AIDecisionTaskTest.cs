using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ZetaGames.RPG {
    public class AIDecisionTaskTest : MonoBehaviour {
        private NavMeshAgent agent;
        private Dictionary<ResourceType, int> inventory;
        private ResourceManager resourceManager;
        private Collider2D destinationCollider;
        private Vector2 startingPosition;
        private int layerMask;
        private bool lookingForWood = true;
        private TaskManager taskManager;

        private void Awake() {
            // TASK MANAGER SETUP
            taskManager = GetComponent<TaskManager>();

            // NAVMESH SETUP
            agent = GetComponent<NavMeshAgent>();
            agent.updateUpAxis = false;
            agent.updateRotation = false;

            // INVENTORY SETUP
            resourceManager = FindObjectOfType<ResourceManager>();
            resourceManager.addNPC(gameObject.GetInstanceID());
            inventory = resourceManager.getNpcInventory(gameObject.GetInstanceID());

            // FOR TESTING
            startingPosition = transform.position;
        }

        private void Start() {
            lookForWood();
        }

        private void Update() {
            if (taskManager.TaskList.Count == 0 && lookingForWood) {
                lookForWood();
            }
        }

        private void lookForWood() {
            // Am I looking for wood?
            if (lookingForWood) {
                // Do I have enough wood already? (I want two wood)
                if (inventory[ResourceType.Wood] < 2) {
                    Debug.Log(name + ": I don't have enough wood");
                    // Look for free wood on the ground or a tree if no free wood is found
                    lookForDroppedWood();
                } else {
                    if (lookingForWood) {
                        Debug.Log(name + ": I have enough wood now");
                        lookingForWood = false;
                        //agent.destination = startingPosition;
                        taskManager.AddNavMeshAgentMoveTask(startingPosition);
                    }
                }
            }
        }

        private void lookForDroppedWood() {
            layerMask = 1 << 6; // layer mask 6 (dropped resources)
            destinationCollider = FindNearestCollider(ResourceType.Wood.ToString(), layerMask);
            if (destinationCollider != null) {
                //agent.destination = destinationCollider.gameObject.transform.position;
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
                //agent.destination = destinationCollider.gameObject.transform.position;
                taskManager.AddNavMeshAgentMoveTask(destinationCollider.gameObject.transform.position);
                Debug.Log(name + ": I found a tree!");

                /******************
                 Add function to start
                chopping tree down
                 *********************/

                lookingForWood = false;
            } else {
                //agent.destination = startingPosition;
                taskManager.AddNavMeshAgentMoveTask(startingPosition);
                Debug.Log(name + ": I couldn't find any trees");
                lookingForWood = false;
            }
        }

        /*
        private bool atDestination(Vector2 destination) {
            // TODO: write actual code
            return false;
        }

        private bool targetExists(Collider2D target) {
            // TODO: write actual code
            if (destinationCollider != null) {
                
            }
            return false;
        }
        */

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
