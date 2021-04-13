using System.Linq;
using UnityEngine;

namespace ZetaGames.RPG {
    public class SearchForResourceNode {
        /*
        public bool isFinished { get => finished; }
        public bool isInterruptable { get => true; }
        private bool finished;
        private readonly AIBrain npcBrain;
        private Transform charTransform;
        private ResourceType resourceType;
        private float searchRange;
        private int layerMask;

        public SearchForResourceNode(AIBrain npcBrain, ResourceType resourceType, float searchRange, int layerMask) {
            this.npcBrain = npcBrain;
            this.resourceType = resourceType;
            this.searchRange = searchRange;
            this.layerMask = layerMask;
            charTransform = npcBrain.gameObject.transform;
        }
        public void Tick() {
            npcBrain.resourceNodeTarget = ZetaUtilities.FindNearestCollider(charTransform.position, resourceType.ToString(), searchRange, layerMask).GetComponentInChildren<GatherableResource>();

            if (npcBrain.resourceNodeTarget != null) {
                npcBrain.destination = npcBrain.resourceNodeTarget.transform.position;
            }

            if (npcBrain.debugLogs) {
                Debug.Log("npcBrain.resourceNodeTarget = " + npcBrain.resourceNodeTarget.transform.parent.transform.parent.name);
            }
        }

        public void OnEnter() {
            npcBrain.ResetAgent();
            if (npcBrain.debugLogs) {
                Debug.Log("SearchForResources.OnEnter()");
            }
        }
        public void OnExit() {
            if (npcBrain.debugLogs) {
                Debug.Log("SearchForResources.OnExit()");
            }
        }
        */
    }
}

