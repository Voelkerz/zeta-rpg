using System.Linq;
using UnityEngine;

namespace ZetaGames.RPG {
    public class SearchForResource : IState {
        public bool isFinished { get => finished; }
        public bool isInterruptable { get => true; } //timeSearched > npcBrain.personality.resourceMaxSearchTime; }
        private static readonly int shouldMove = Animator.StringToHash("move");
        private bool finished;
        private readonly AIBrain npcBrain;
        private ResourceType resourceType;
        private float searchRange;
        private Collider2D targetCollider;
        //private float timeSearched;
        private float timer;
        private string lastKnownResourceLocation;
        private Vector2 lastPosition;
        private bool activelySearching;
        private float wanderTimer;

        public SearchForResource(AIBrain npcBrain) {
            this.npcBrain = npcBrain;
        }

        public void Tick() {
            //timeSearched += Time.deltaTime;
            timer += Time.deltaTime;

            //Check for resources every 1 second while in this state to reduce CPU usage
            if (timer > 1f && !activelySearching) {
                timer = 0;
                // if we don't have a resource target and we're not carrying something, get one
                if (npcBrain.resourceTarget == null) {
                    if (!npcBrain.npcInventory.IsCarryingSomething()) {
                        GetTarget();
                    }
                } else if (!npcBrain.resourceTarget.activeSelf) {
                    // or if we do have a target and it's not active, nullify
                    npcBrain.resourceTarget = null;
                } else {
                    if (npcBrain.resourceTarget.TryGetComponent(out DroppedResource resource)) {
                        resource.PickUp();
                    }
                }
            } else if (activelySearching && npcBrain.animator.GetCurrentAnimatorStateInfo(0).IsTag("idle")) {
                activelySearching = false;
            }

            if (Vector2.Distance(npcBrain.transform.position, npcBrain.destination) > 0.01f) {
                if (npcBrain.debugLogs) {
                    Debug.Log("SearchForResourceDrop.Tick(): have destination, calling animator move()");
                }

                //npcBrain.animationManager.Move();

                if (Vector2.Distance(npcBrain.transform.position, lastPosition) <= 0f) {
                    npcBrain.timeStuck += Time.deltaTime;
                } else {
                    npcBrain.timeStuck = 0;
                }

                lastPosition = npcBrain.transform.position;
            } else {
                //npcBrain.animator.SetBool(shouldMove, false);

                /*
                Collider2D[] colliders = Physics2D.OverlapCircleAll(npcBrain.transform.position, 0.5f, 1 << 6);

                if (colliders.Length > 0) {
                    foreach (Collider2D collider in colliders) {
                        if (collider.TryGetComponent(out DroppedResource resource)) {
                            resource.PickUp();
                            break;
                        }
                    }
                }
                */
            }
        }

        public void OnEnter() {
            if (npcBrain.debugLogs) {
                Debug.Log("SearchForResourceDrop.OnEnter()");
            }
            npcBrain.ResetAgent();
            npcBrain.timeStuck = 0f;
            //timeSearched = 0;
            timer = 0;
            wanderTimer = 10;
            resourceType = npcBrain.resourceNeeded;
            searchRange = npcBrain.personality.resourceMaxSearchRange;
            lastKnownResourceLocation = "lastKnown" + resourceType.ToString() + "Location";
            npcBrain.destination = npcBrain.transform.position;
            npcBrain.resourceTarget = null;
        }

        public void OnExit() {
            npcBrain.timeStuck = 0f;
        }

        private void GetTarget() {
            // search for free dropped resources
            targetCollider = ZetaUtilities.FindNearestCollider(npcBrain.transform.position, resourceType.ToString(), searchRange, 1 << 6);
            // if a dropped resource is found, make it our target and destination
            if (targetCollider != null) {
                if (npcBrain.debugLogs) {
                    Debug.Log("SearchForResourceDrop.Tick(): dropped resource found");
                }
                npcBrain.resourceTarget = targetCollider.gameObject;
                npcBrain.destination = targetCollider.transform.position;
                npcBrain.navMeshAgent.SetDestination(npcBrain.destination);
            } else {
                if (npcBrain.debugLogs) {
                    Debug.Log("SearchForResourceDrop.Tick(): dropped resource not found. Looking for resource node.");
                }
                // otherwise, look for a resource node to harvest instead
                targetCollider = ZetaUtilities.FindNearestCollider(npcBrain.transform.position, resourceType.ToString(), searchRange, 1 << 7);
                // if a resource node is found, make it our target and destination
                if (targetCollider != null) {
                    if (npcBrain.debugLogs) {
                        Debug.Log("SearchForResourceDrop.Tick(): resource node found");
                    }

                    if (targetCollider.transform.parent.gameObject.TryGetComponent(out HarvestableResource harvestableResource)) {
                        npcBrain.resourceTarget = harvestableResource.gameObject;
                        npcBrain.destination = targetCollider.transform.position;
                        npcBrain.navMeshAgent.SetDestination(npcBrain.destination);
                        // remember the last location of a resource node for future reference
                        npcBrain.npcMemory.AddMemory(lastKnownResourceLocation, npcBrain.resourceTarget.transform.position);
                    }
                } else {
                    if (npcBrain.debugLogs) {
                        Debug.Log("SearchForResourceDrop.Tick(): resource node not found. Checking memory for last known resource location.");
                    }
                    // otherwise, travel to last known location of the resource if there is one remembered and not too far away
                    if (npcBrain.npcMemory.ContainsMemory(lastKnownResourceLocation)) {
                        Vector2 memoryLocation = (Vector3)npcBrain.npcMemory.RetrieveMemory(lastKnownResourceLocation);

                        if (npcBrain.debugLogs) {
                            Debug.Log("SearchForResourceDrop.Tick(): memory found");
                        }

                        if (Vector2.Distance(npcBrain.transform.position, memoryLocation) < npcBrain.personality.maxDistanceFromPosition) {
                            npcBrain.destination = memoryLocation;
                            npcBrain.navMeshAgent.SetDestination(npcBrain.destination);
                            npcBrain.npcMemory.RemoveMemory(lastKnownResourceLocation); // remove memory so npc doesn't keep trying to go to same spot if there are no resources there
                        } else {
                            //TODO: Go to a shop instead if the last known location is too far away
                        }
                    } else {
                        if (npcBrain.debugLogs) {
                            Debug.Log("SearchForResourceDrop.Tick(): memory not found");
                        }
                        // wander around to look for resources
                        if (wanderTimer > 10) {
                            wanderTimer = 0;
                            Vector2 newPos = ZetaUtilities.RandomNavSphere(npcBrain.transform.position, npcBrain.personality.resourceMaxWanderRange, -1);
                            activelySearching = true;
                            npcBrain.destination = newPos;
                            npcBrain.navMeshAgent.SetDestination(npcBrain.destination);
                        } else {
                            wanderTimer++;
                        }
                    }
                }
            }
        }
    }
}

