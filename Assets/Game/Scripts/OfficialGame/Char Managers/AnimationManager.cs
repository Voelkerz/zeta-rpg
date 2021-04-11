using System;
using UnityEngine;
using UnityEngine.AI;

namespace ZetaGames.RPG {

    /**********************************
     * Rework this into state machine
     * so it doesn't constantly use 
     * update()
    ***********************************/

    public class AnimationManager : MonoBehaviour {
        private NavMeshAgent navMeshAgent;
        private Animator animator;
        private AIBrain npcBrain;
        private static readonly int shouldMove = Animator.StringToHash("move");
        private static readonly int velocityX = Animator.StringToHash("velx");
        private static readonly int velocityY = Animator.StringToHash("vely");
        private static readonly int lastDirection = Animator.StringToHash("lastDirection");
        private Vector2 smoothDeltaPosition = Vector2.zero;
        private Vector2 velocity = Vector2.zero;
        private float lastKnownDirection;
        private Vector2 lastPosition;

        // Start is called before the first frame update
        void Awake() {
            // SETUP NAVMESH AGENT
            navMeshAgent = GetComponentInParent<NavMeshAgent>();
            navMeshAgent.updateUpAxis = false;
            navMeshAgent.updateRotation = false;
            navMeshAgent.updatePosition = false;

            // SETUP ANIMATOR
            animator = GetComponent<Animator>();
        }

        public void Move() {
            //if (Vector2.Distance(npcBrain.transform.position, lastPosition) <= 0f) {
            //    npcBrain.timeStuck += Time.deltaTime;
            //}
            //lastPosition = npcBrain.transform.position;

            Vector2 worldDeltaPosition = navMeshAgent.nextPosition - navMeshAgent.gameObject.transform.position;

            // Map 'worldDeltaPosition' to local space
            float dx = Vector2.Dot(navMeshAgent.gameObject.transform.right, worldDeltaPosition);
            float dy = Vector2.Dot(navMeshAgent.gameObject.transform.forward, worldDeltaPosition);
            Vector2 deltaPosition = new Vector2(dx, dy);

            // Low-pass filter the deltaMove
            float smooth = Mathf.Min(1.0f, Time.deltaTime / 0.15f);
            smoothDeltaPosition = Vector2.Lerp(smoothDeltaPosition, deltaPosition, smooth);

            // Update velocity if time advances
            if (Time.deltaTime > 1e-5f)
                velocity = smoothDeltaPosition / Time.deltaTime;

            bool move = velocity.magnitude > 0.1f && navMeshAgent.remainingDistance > navMeshAgent.radius;

            // update last known direction for idle animations
            if (deltaPosition.x > 0 && deltaPosition.y < 0) {
                lastKnownDirection = 0; // facing NE
                velocity.x = 1;
                velocity.y = -1;
            } else if (deltaPosition.x < 0 && deltaPosition.y > 0) {
                lastKnownDirection = 1; // facing SW
                velocity.x = -1;
                velocity.y = 1;
            } else if (deltaPosition.x < 0 && deltaPosition.y < 0) {
                lastKnownDirection = 2; // facing NW
                velocity.x = -1;
                velocity.y = -1;
            } else if (deltaPosition.x > 0 && deltaPosition.y > 0) {
                lastKnownDirection = 3; // facing SE
                velocity.x = 1;
                velocity.y = 1;
            }

            // Update animation parameters
            animator.SetBool(shouldMove, move);
            animator.SetFloat(velocityX, velocity.x);
            animator.SetFloat(velocityY, velocity.y);
            animator.SetFloat(lastDirection, lastKnownDirection);
        }

        void OnAnimatorMove() {
            // Update position to agent position
            transform.parent.transform.position = navMeshAgent.nextPosition;
        }
    }
}

