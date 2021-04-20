using System;
using UnityEngine;
using UnityEngine.AI;

namespace ZetaGames.RPG {
    public class AnimationManager : MonoBehaviour {
        private NavMeshAgent navMeshAgent;
        private Animator animator;
        private static readonly int velocityX = Animator.StringToHash("velx");
        private static readonly int velocityY = Animator.StringToHash("vely");
        private static readonly int shouldMove = Animator.StringToHash("move");
        private static readonly int lastDirection = Animator.StringToHash("lastDirection");
        private Vector2 smoothDeltaPosition = Vector2.zero;
        private Vector2 velocity = Vector2.zero;
        private bool move;
        
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
            Vector3 worldDeltaPosition = navMeshAgent.nextPosition - navMeshAgent.gameObject.transform.position;

            // Map 'worldDeltaPosition' to local space
            float dx = Vector3.Dot(navMeshAgent.gameObject.transform.right, worldDeltaPosition);
            float dy = Vector3.Dot(navMeshAgent.gameObject.transform.forward, worldDeltaPosition);
            Vector3 deltaPosition = new Vector3(dx, dy);

            // Low-pass filter the deltaMove
            float smooth = Mathf.Min(1.0f, Time.deltaTime / 0.15f);
            smoothDeltaPosition = Vector3.Lerp(smoothDeltaPosition, deltaPosition, smooth);

            // Update velocity if time advances
            if (Time.deltaTime > 1e-5f)
                velocity = smoothDeltaPosition / Time.deltaTime;

            move = velocity.magnitude > 0.1f && navMeshAgent.remainingDistance * 2 > navMeshAgent.radius;
            //Debug.Log("move: " + move + " || remainingDistance: " + navMeshAgent.remainingDistance);

            // update last known direction for idle animations
            if (deltaPosition.x > 0 && deltaPosition.y < 0) {
                //lastKnownDirection = 0; // facing NE
                //velocity.x = 1;
                //velocity.y = -1;
                animator.SetFloat(velocityX, 1f);
                animator.SetFloat(velocityY, -1f);
                animator.SetFloat(lastDirection, 0f);
            } else if (deltaPosition.x < 0 && deltaPosition.y > 0) {
                //lastKnownDirection = 1; // facing SW
                //velocity.x = -1;
                //velocity.y = 1;
                animator.SetFloat(velocityX, -1f);
                animator.SetFloat(velocityY, 1f);
                animator.SetFloat(lastDirection, 1f);
            } else if (deltaPosition.x < 0 && deltaPosition.y < 0) {
                //lastKnownDirection = 2; // facing NW
                //velocity.x = -1;
                //velocity.y = -1;
                animator.SetFloat(velocityX, -1f);
                animator.SetFloat(velocityY, -1f);
                animator.SetFloat(lastDirection, 2f);
            } else if (deltaPosition.x > 0 && deltaPosition.y > 0) {
                //lastKnownDirection = 3; // facing SE
                //velocity.x = 1;
                //velocity.y = 1;
                animator.SetFloat(velocityX, 1f);
                animator.SetFloat(velocityY, 1f);
                animator.SetFloat(lastDirection, 3f);
            }

            // Update animation parameters
            animator.SetBool(shouldMove, move);
            //animator.SetFloat(velocityX, velocity.x);
            //animator.SetFloat(velocityY, velocity.y);
            //animator.SetFloat(lastDirection, lastKnownDirection);
        }

        void OnAnimatorMove() {
            // Update position to agent position
            transform.parent.transform.position = navMeshAgent.nextPosition;
        }
    }
}

