using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;

/*This is an example of a new set of Tasks im working on for a game.
Note how they will inherit from Task.
My personal choice here will be to make them Inherit from HumanTasks also so I can define a few extra bits
that define what a human can and cant do or how it might do it. */


public class NavMeshTask : Task {

    //The Game World Coordinates for the NavMeshAgent to head towards.
    public Vector3 DestinationPosition { get; set; }
    //Agent reference.
    public NavMeshAgent Agent;
    //Reference to the GameObject that is using this Task. Required!
    public GameObject ThisGameObject { get; set; }

    //Constructor
    public NavMeshTask() {
        Initialised = false;
    }

    //Called to check if the task has been setup correctly, returns true if everything seems right.
    private bool SetupCheck() {
        if (Agent == null || Priority == -1 || TaskID == 0 || ThisGameObject == null) {
            Debug.LogWarning("NavMeshTask - Task was not setup correctly!");
            return false;
        } else {
            return true;
        }
    }

    //This tasks implementation of Valid() simply relays the output of the function SetupCheck() waste of a call right now but maybe useful for later Tasks?
    public override bool Valid {
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
        Agent.ResetPath();
        //IMPORTANT that this is now set to true. The TaskManager relies on this variable.
        Initialised = true;
    }

    public override void OnTaskStart() {
        Debug.Log("NavMeshTask - NavMeshAgent moving to: " + DestinationPosition);
    }

    //Execute() needs to be called in update of the TaskManager. Setting the destination doesn't need to be done in each update.
    public override void Execute() {
        if (!Agent.hasPath) {
            Agent.destination = DestinationPosition;
        }
        if (Started == true) {
            if (Agent.pathPending) {
                Debug.Log("NavMeshTask - Path is being calculated.");
            } else {
                if (Agent.pathStatus == NavMeshPathStatus.PathInvalid || Agent.pathStatus == NavMeshPathStatus.PathPartial) {
                    Debug.Log("NavMeshTask - Path invalid.");
                    //TODO: Handle invalid pathing.
                }
                if (Agent.pathStatus == NavMeshPathStatus.PathComplete) {
                    //Debug.Log ("MoveTask - Path complete."); << Loads of Logs!
                    if (Agent.remainingDistance == 0) {
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
        Initialised = false;
        Started = false;
        Agent.ResetPath();
    }

}//END OF CLASS