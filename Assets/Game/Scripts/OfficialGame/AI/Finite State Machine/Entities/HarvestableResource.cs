using System;
using UnityEngine;
using UnityEngine.AI;

namespace ZetaGames.RPG {
    public class HarvestableResource : MonoBehaviour {

        public ResourceType resourceType;
        private int totalHitsToDeplete = 15;
        private int hitCountdown = 15;
        public bool IsDepleted => hitCountdown <= 0;

        private void OnEnable() {
            hitCountdown = totalHitsToDeplete;
        }

        public bool Hit(int hitAmount) {
            if (hitCountdown <= 0) {
                Destroy(gameObject.transform.parent.gameObject);
                return false;
            } else {
                hitCountdown =- hitAmount;
                return true;
            }
        }

    }
}

