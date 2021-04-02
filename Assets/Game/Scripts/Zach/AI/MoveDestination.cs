using UnityEngine;
using UnityEngine.AI;

namespace ZetaGames.RPG {
    public class MoveDestination : MonoBehaviour {

        public Transform goal;
        private NavMeshAgent agent;

        void Start() {
            agent = GetComponent<NavMeshAgent>();
        }

        private void Update() {
            agent.destination = goal.position;
        }
    }
}
