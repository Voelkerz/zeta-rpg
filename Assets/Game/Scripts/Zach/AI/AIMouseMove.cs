using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ZetaGames.RPG {
    public class AIMouseMove : MonoBehaviour {
        private NavMeshAgent agent;

        private void Start() {
            agent = GetComponent<NavMeshAgent>();
        }

        private void Update() {
            if (Input.GetMouseButtonUp(0)) {
                var target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                target.z = 0;
                agent.destination = target;
            }
        }
    }
}
