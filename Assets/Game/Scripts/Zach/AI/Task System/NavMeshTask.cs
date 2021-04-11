using UnityEngine;
using UnityEngine.AI;

namespace ZetaGames.RPG {
    public class NavMeshTask : Task {

        //The Game World Coordinates for the NavMeshAgent to head towards.
        public Vector3 destinationPosition { get; set; }
        //Agent reference.
        public NavMeshAgent agent;
        //Reference to the GameObject that is using this Task. Required!
        public GameObject thisGameObject { get; set; }

        //Constructor
        public NavMeshTask() {
            initialised = false;
        }

        //Called to check if the task has been setup correctly, returns true if everything seems right.
        private bool SetupCheck() {
            if (agent == null || priority == -1 || taskID == 0 || thisGameObject == null) {
                Debug.LogWarning("NavMeshTask - Task was not setup correctly!");
                return false;
            } else {
                return true;
            }
        }

        //This tasks implementation of Valid() simply relays the output of the function SetupCheck() waste of a call right now but maybe useful for later Tasks?
        public override bool valid {
            get {
                if (!SetupCheck()) {
                    return false;
                } else {
                    return true;
                }
            }
        }

        //This Tasks implementation of Initilise() simply sets the NavMeshAgents 'DestinationPosition'.
        public override void Initialise() {
            agent.ResetPath();
            //IMPORTANT that this is now set to true. The TaskManager relies on this variable.
            initialised = true;
        }

        public override void OnTaskStart() {
            Debug.Log("NavMeshTask - NavMeshAgent moving to: " + destinationPosition);
        }

        //Execute() needs to be called in update of the TaskManager. Setting the destination doesn't need to be done in each update.
        public override void Execute() {
            if (!agent.hasPath) {
                agent.destination = destinationPosition;
            }
            if (started == true) {
                if (agent.pathPending) {
                    Debug.Log("NavMeshTask - Path is being calculated.");
                } else {
                    if (agent.pathStatus == NavMeshPathStatus.PathInvalid || agent.pathStatus == NavMeshPathStatus.PathPartial) {
                        Debug.Log("NavMeshTask - Path invalid.");
                        //TODO: Handle invalid pathing.
                    }
                    if (agent.pathStatus == NavMeshPathStatus.PathComplete) {
                        //Debug.Log ("MoveTask - Path complete."); << Loads of Logs!
                        if (agent.remainingDistance != Mathf.Infinity && agent.remainingDistance == 0) {
                            Debug.Log("NavMeshTask - Destination Reached.");
                            _finished = true;
                        } else {
                            //Debug.Log ("Destination not reached"); << Loads of Logs!
                        }
                    } else {
                    }
                }
            } else {
                Debug.LogWarning("NavMeshTask - This Task has not yet begun! (Something went very wrong?!)");
                //TODO: Work out if this can even happen now I have reorganised the TaskManager flow.
            }
        }

        public override bool Finished() {
            return _finished;
        }

        public override void Reset() {
            initialised = false;
            started = false;
            agent.ResetPath();
        }
    }
}
