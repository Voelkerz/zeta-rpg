using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ZetaGames.RPG {
    public class NPCManager : MonoBehaviour {

        private NavMeshAgent agent;

        private void Awake() {
            agent = GetComponent<NavMeshAgent>();
            agent.updateUpAxis = false;
            agent.updateRotation = false;
            agent.updatePosition = false;
        }
    }


}

