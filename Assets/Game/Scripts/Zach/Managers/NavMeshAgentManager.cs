using UnityEngine;
using UnityEngine.AI;

namespace ZetaGames.RPG {
    public class NavMeshAgentManager : MonoBehaviour {
        private NavMeshAgent agent;

        private void Start() {
            // NAVMESH SETUP
            agent = GetComponent<NavMeshAgent>();
            agent.updateUpAxis = false;
            agent.updateRotation = false;
            agent.updatePosition = false;
        }
    }
}

