using UnityEngine;
using System.Collections;
using System;

namespace ZetaGames.RPG {
    public class TimedTask : Task {


        /* TimeInList should not be modified directly, it is updated by the TaskManager.
        TimeAsCurrentTask shouldn't either.

        ExpiryTime should be set when the Task is setup, this is the length of time the Task can sit in the TaskManager.
        TTL is the Time To Live as the current Task. This will only countdown once the Task is Valid, Initialised and Started.
        */
        public float timeInList = 0f;

        public float expiryTime = -1;

        public float timeAsCurrentTask = 0f;

        public float ttl= -1;

        //This functions checks the criteria listed below, if any of them fail, it returns false; and when asked by Valid, flags the task as (not)Valid.
        private bool SetupCheck() {
            if (priority == -1 || taskID == 0) {
                if (expiryTime == -1 && ttl == -1) {
                    return false;
                }
                Debug.LogWarning("TimedTask - Task was not setup correctly!");
                return false;
            } else {
                return true;
            }
        }
        //This simply calls the function above, you can put whatever you want in here to decide if the Task is valid and not use the function above.
        public override bool valid {
            get {
                if (!SetupCheck()) {
                    return false;
                } else {
                    return true;
                }
            }
        }

        //Do your Initialisation in here!
        public override void Initialise() {
            initialised = true;
        }

        //Chuck all your normal functionality in here!
        public override void Execute() {

        }


        /* Something important to note here: If you don't setup a TimedTask with a timer then it may just hang in the first position of the list.
        EDIT: For the sake of having foolproof code I have updated the Valid check for you.

        */
        public override bool Finished() {
            if (expiryTime != -1 && timeInList >= expiryTime) {
                return true;
            }
            if (ttl != -1 && timeAsCurrentTask >= ttl) {
                return true;
            } else {
                return false;
            }
        }

        public override void Reset() {
            initialised = false;
            started = false;
            //Remember to reset the current Task timer ;P
            timeAsCurrentTask = 0f;
        }
    }
}
