using System;
using UnityEngine;
using UnityEngine.AI;

namespace ZetaGames.RPG {
    public class GatherableResource : MonoBehaviour {

        public ResourceType resourceType;
        private int totalHitsToDeplete = 15;
        private int hitCountdown = 15;
        public bool IsDepleted => hitCountdown <= 0;

        private void OnEnable() {
            hitCountdown = totalHitsToDeplete;
        }

        public bool Hit(int hitAmount) {
            if (hitCountdown <= 0) {
                Destroy(gameObject.transform.parent.transform.parent.gameObject);
                return false;
            } else {
                hitCountdown =- hitAmount;
                return true;
            }
        }

        [ContextMenu("Snap")]
        private void Snap() {
            if (NavMesh.SamplePosition(transform.position, out var hit, 5f, NavMesh.AllAreas)) {
                transform.position = hit.position;
            }
        }

        public void SetHitCountdown(int amount) => hitCountdown = amount;
    }
}

