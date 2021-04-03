using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;


public class ComplexTask : Task {

    //List of Tasks to complete.
    public List<Task> ComplexTaskList;
    //Constructor
    public ComplexTask() {
        ComplexTaskList = new List<Task>();
    }

    //
    void ProcessComplexTask() {

        UpdateTimedTaskCounters();
        //If this task is not initialised, initialise it.
        if (ComplexTaskList[0].Valid) {
            //If its not initialised, intialise it.
            if (ComplexTaskList[0].Initialised) {
                //If the task isn't finished, execute it.
                if (!ComplexTaskList[0].Finished()) {
                    if (ComplexTaskList[0].Started == false) {
                        ComplexTaskList[0].Started = true;
                        OnTaskStart(ComplexTaskList[0]);
                    }
                    ComplexTaskList[0].Execute();
                } else if (ComplexTaskList[0].Finished()) {
                    Debug.Log("TaskManager - Task finished, removing!");
                    ComplexTaskList.RemoveAt(0);
                }
            } else {
                ComplexTaskList[0].Initialise();
            }
        } else if (!ComplexTaskList[0].Valid) {
            Debug.LogWarning("TaskManager - Invalid Task detected, removing!");
            ComplexTaskList.RemoveAt(0);
        }
    }

    void UpdateTimedTaskCounters() {

        List<TimedTask> RemovalList = new List<TimedTask>();
        foreach (TimedTask t in ComplexTaskList.OfType<TimedTask>()) {
            //Update the TimeInList (time sat in the list).
            t.TimeInList += Time.deltaTime;

            //If the current task has begun update the TimeAsCurrentTask.
            if (t.Started == true) {
                t.TimeAsCurrentTask += Time.deltaTime;
            }
            //If the current task now thinks its done, then remove it!
            if (t.Started == false && t.Finished()) {
                t.OnTaskEnd();
                RemovalList.Add(t);
            }
        }
        foreach (TimedTask rm in RemovalList) {
            Debug.Log("TaskManager - Task expired: " + rm);
            ComplexTaskList.Remove(rm);
        }
    }

    void OnTaskStart(Task TaskReference) {
        Debug.Log("TaskManager - Executing current task.");
        Debug.Log("TaskManager - Tasks in queue: " + ComplexTaskList.Count + ".");
    }


    private bool SetupCheck() {
        if (Priority == 0 || TaskID == 0) {
            Debug.LogWarning("ComplexTask - Task was not setup correctly!");
            return false;
        } else {
            return true;
        }
    }

    public override bool Valid {
        get {
            if (!SetupCheck()) {
                return false;
            } else {
                return true;
            }
        }
    }

    //Do your Initialistion stuff in here!
    public override void Initialise() {
        Initialised = true;
    }

    //Execute() needs to be called in update of the TaskManager.
    public override void Execute() {
        ProcessComplexTask();
    }

    //This simply checks to see if the list is empty. If it is then the Task is obviously done!
    public override bool Finished() {
        if (ComplexTaskList.Count <= 0) {
            return true;
        } else {
            return false;
        }
    }

    public override void Reset() {
        ComplexTaskList[0].Reset();
    }

}
