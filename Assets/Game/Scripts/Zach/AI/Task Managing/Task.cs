using UnityEngine;
using System.Collections;
using System;

public abstract class Task {

    //Returns true if the conditions declared by the Task are met.
    public abstract bool Valid { get; }

    //Returns true if the Task has had Initialise() called.
    public bool Initialised { get; set; }

    //Returns True if the Task has been started.
    public bool Started { get; set; }

    //Used for sorting Tasks.
    public int Priority { get; set; }
    public int TaskID { get; set; }

    //Constructor.
    public Task() {
        Initialised = false;
        Started = false;
    }

    //To be called before Execute or the Task will (probably) fail, depending on whether the task needs to initialise at all.
    public abstract void Initialise();

    //OnTaskStart() is called when the task is Valid AND Initialised!
    public virtual void OnTaskStart() { }

    //Execute() needs to be called in update of the TaskManager. This will probably hold the majority of the game logic for a task.
    //It is executed every update of the TaskManager, use this function as if you were to use Update();
    public abstract void Execute();

    //Allows the TaskManager to check if a task has finished, each task defines it's own rules as to what finished means.
    public abstract bool Finished();
    public bool _finished = false;

    //OnTaskEnd() is called after the Task decides or is told it is finished.
    public virtual void OnTaskEnd() { }

    public virtual void Reset() {
        Initialised = false;
        Started = false;
        Debug.Log("Reset called");
    }

}

///////////////////////////////////////////////////////////////
//Purely for copy and pasting!

public class TaskTemplate : Task {

    public override bool Valid {
        get {
            throw new NotImplementedException();
        }
    }

    public override void Initialise() {
        throw new NotImplementedException();
    }

    //Execute() needs to be called in update of the TaskManager.
    public override void Execute() {

    }

    public override bool Finished() {
        throw new NotImplementedException();
    }

}

////////////////////////////////////////////////////////////////
