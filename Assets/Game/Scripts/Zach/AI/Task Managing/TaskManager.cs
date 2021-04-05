using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;

namespace ZetaGames.RPG {
    public class TaskManager : MonoBehaviour {

        #region TaskManager Functionality

        public List<Task> TaskList;

        /*You can simply pause the TaskManager by toggling this bool, note this won't actually stop some things from happening.
        For example, a NavMeshAgent will keep moving to its last target but not the next one, you could handle this in your Tasks though I'm sure.
        Possibly re-organise how Update() handles the pausing mechanic. Possibly checking to see if the TaskManager is Paused, then call a Pause()
        method on the Task and handle it in the task. NOTE: In the HumanTasks.cs See NavMeshTask.Reset() for an idea. ;P
         */
        public bool Paused = false;


        /*This switches the sorting by priority on or off. This is evaluated at the END of the first Task in the list.
        This is because sorting the list every Update is riduculous, also it introduces problems with adding tasks, whereby
        a new Task could push itself to the top of the list, leaving the previous task unfinished and pushed down the stack.
        This is fine as we can call Reset() on that task and everything would be fine, but sorting the list every update is
        just too much work in my eyes, an alternative is to check if the list count has changed each update, if it has then Reset()
        the current Task, SortListByPriority() and begin again.

        My attitude is that if a Task is SO important it MUST be first on the list IMMEDIATELY then you should handle these events
        yourself as they break the natural flow of the system. I have included the AddTaskAtBeginning(Task); to show how to set
        up these tasks. It's quite simple.
        */

        public bool PrioritySort = true;

        void Awake() {
            TaskList = new List<Task>();
        }

        void SortListByPriority() {
            if (TaskList.Count > 0) {
                TaskList = TaskList.OrderBy(x => x.priority).Reverse().ToList();
            }
        }

        /*ProcessList() Handles the standard processing of the list including Validity checks, Initialisation and Execution of Tasks.
        It also calls OnTaskStart() and OnTaskEnd() at the appropriate times. */
        void ProcessList() {
            //If this Task decides it is invalid, then delete it.
            if (TaskList[0].valid) {
                //If its not initialised, intialise it.
                if (TaskList[0].initialised) {
                    //If the task isn't finished, execute it.
                    if (!TaskList[0].Finished()) {
                        if (TaskList[0].started == false) {
                            TaskList[0].started = true;
                            TaskList[0].OnTaskStart();
                        }
                        TaskList[0].Execute();
                    } else if (TaskList[0].Finished()) {
                        Debug.Log("TaskManager - Task finished, removing!");
                        //Call OnTaskEnd() and then remove the task.
                        TaskList[0].OnTaskEnd();
                        TaskList.RemoveAt(0);
                        //If PrioritySort is on, sort the list now, before we start the next task.
                        if (PrioritySort) {
                            SortListByPriority();
                        }
                    }
                } else {
                    TaskList[0].Initialise();
                }
            } else if (!TaskList[0].valid) {
                Debug.LogWarning("TaskManager - Invalid Task detected, removing!");
                TaskList.RemoveAt(0);
            }
        }

        #region TimedTask Handling
        /* This function is called in update, it simply adds time to the TimedTask's counter and asks if it needs to be killed off. Simple.
        Note the reoccuring theme here where-by the TaskManager ASKS the Task if it is finished. The TaskManager doesn't make that decision.*/

        void UpdateTimedTaskCounters() {

            List<TimedTask> RemovalList = new List<TimedTask>();
            foreach (TimedTask t in TaskList.OfType<TimedTask>()) {
                //Update the TimeInList (time sat in the list).
                t.timeInList += Time.deltaTime;

                //If the current task has begun update the TimeAsCurrentTask.
                if (t.started == true) {
                    t.timeAsCurrentTask += Time.deltaTime;
                }
                //If the current task now thinks its done, then remove it!
                if (t.started == false && t.Finished()) {
                    t.OnTaskEnd();
                    RemovalList.Add(t);
                }
            }
            foreach (TimedTask rm in RemovalList) {
                Debug.Log("TaskManager - Task expired: " + rm);
                TaskList.Remove(rm);
            }
        }

        #endregion

        void Update() {
            if (!Paused) {
                if (TaskList.Count > 0) {
                    //StartCoroutine("UpdateTimedTaskCounters"); << I'm not completely sure if this is necessary, possibly with hundreds of objects in the scene?
                    UpdateTimedTaskCounters();
                    ProcessList();
                } else {
                    Debug.Log("TaskManager - TaskList is empty!");
                }
            }
        }
        #endregion

        #region TaskManager Utilities

        /* These are just some samples of what can be done with this system. Some of them might be useful others not. I have tried
        to keep them as generic as possible, the NavMeshAgent task down there is a filaing of this desire, but i was working on it
        and thought I would include it anyhow =].
        */

        public void AddTaskAtBeginning(Task t) {
            Paused = true;
            if (TaskList.Count > 0) {
                TaskList[0].Reset();
                TaskList.Insert(0, t);

            } else {
                TaskList.Add(t);
            }
            Paused = false;
        }

        public void AddNavMeshAgentMoveTask(Vector3 MoveTarget) {

            NavMeshTask NewTask = new NavMeshTask() {
                taskID = 1,
                priority = 1,
                thisGameObject = gameObject,
                agent = gameObject.GetComponent<NavMeshAgent>(),
                destinationPosition = MoveTarget
            };
            TaskList.Add(NewTask);
        }

        /* Here you can see the setup of an example task with a TTL (Time To Live).
        To create a Task that will end itself, simply set the TTL of a Task that extends TimedTask.
        Note that TTL will only check when it is the ACTIVE task.
        */
        public void AddPause(float t) {

            TimedTask NewTask = new TimedTask() {
                taskID = 1,
                priority = 1,
                ttl = t
            };
            TaskList.Add(NewTask);
        }

        public void AddPauseAtBeginning(float t) {
            TimedTask NewTask = new TimedTask() {
                taskID = 1,
                priority = 1,
                ttl = t
            };
            AddTaskAtBeginning(NewTask);
        }

        /*THIS IS A POINTLESS TASK AND WILL PURELY REPORT THAT IT HAS FINISHED AFTER X AMOUNT OF TIME.
        If you want to make task expirable, simply set the ExpiryTime of a Task that extends TimedTask.
        Note that ExpiryTime, will work at ANY time the Task is in the TaskList.
        */
        public void AddExpirableTask(float t) {
            TimedTask NewTask = new TimedTask() {
                taskID = 1,
                priority = 1,
                expiryTime = t
            };
            TaskList.Add(NewTask);
        }
        #endregion
    }
}

