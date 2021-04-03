using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ZetaGames.RPG {
    public class AIMouseMove : MonoBehaviour {
        private NavMeshAgent agent;

        private void Start() {
            agent = GetComponent<NavMeshAgent>();
            agent.updateUpAxis = false;
            agent.updateRotation = false;
        }

        private void Update() {
            if (Input.GetMouseButtonUp(0)) {
                var target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                target.z = 0;
                agent.destination = target;
            }

            /*
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hitData = Physics2D.Raycast(new Vector2(worldPosition.x, worldPosition.y), Vector2.zero, 0);

            if (hitData && Input.GetMouseButtonDown(0)) {
                agent.SetDestination(hitData.point);
                
                Debug.Log("OnNavMesh: " + agent.isOnNavMesh + " || Destination: " + hitData.point);
            }
            */
        }
    }
}
