using UnityEngine;
using System.Collections;


public class CivilianTest : MonoBehaviour {

    TaskManager TM;

    void Awake() {
        TM = GetComponent<TaskManager>();
    }

    // Use this for initialization
    void Start() {
        TM.AddExpirableTask(15.0f);
        TM.AddNavMeshAgentMoveTask(new Vector3(5, 2, 0));
        TM.AddPause(2.0f);
        TM.AddNavMeshAgentMoveTask(new Vector3(0, -7, 0));
        TM.AddNavMeshAgentMoveTask(new Vector3(5, 2, 0));
        TM.AddNavMeshAgentMoveTask(new Vector3(0, 4, 0));
        TM.AddNavMeshAgentMoveTask(new Vector3(7, -8, 0));

    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.O)) {
            TM.AddPauseAtBeginning(2.0f);
            Debug.Log("Added at start");
        }
        if (Input.GetKeyDown(KeyCode.P)) {
            if (TM.Paused) {
                TM.Paused = false;
            } else TM.Paused = true;
        }
    }
}