using System.Linq;
using UnityEngine;

namespace ZetaGames.RPG {
    public class SearchForResourceDrop : IState {
        public bool isFinished { get => finished; }
        public bool isInterruptable { get => timeSearched > 1f; }
        private bool finished;
        private readonly AIBrain npcBrain;
        private Transform charTransform;
        private ResourceType resourceType;
        private float searchRange;
        private int layerMask;
        public bool attempted;
        private Collider2D targetCollider;
        float timeSearched;

        public SearchForResourceDrop(AIBrain npcBrain, ResourceType resourceType, float searchRange, int layerMask) {
            this.npcBrain = npcBrain;
            this.resourceType = resourceType;
            this.searchRange = searchRange;
            this.layerMask = layerMask;
            charTransform = npcBrain.gameObject.transform;
        }
        public void Tick() {
            attempted = true;
            timeSearched += Time.deltaTime;
            targetCollider = ZetaUtilities.FindNearestCollider(charTransform, resourceType.ToString(), searchRange, layerMask);

            if (targetCollider != null) {
                npcBrain.resourceDropTarget = targetCollider.gameObject.GetComponent<NpcResourcePickup>();
                npcBrain.destination = npcBrain.resourceDropTarget.transform.position;
                //finished = true;
            } else {
                npcBrain.resourceDropTarget = null;
                //if (timeSearched > 0.5) {
                    //finished = true;
                //}
            }
        }

        public void OnEnter() {
            npcBrain.ResetAgent();
            attempted = false;
            //finished = false;
            timeSearched = 0;
        }
        public void OnExit() {
            attempted = false;
        }
    }
}

