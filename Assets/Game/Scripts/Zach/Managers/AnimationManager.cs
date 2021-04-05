using UnityEngine;
using UnityEngine.AI;

namespace ZetaGames.RPG {
    public class AnimationManager : MonoBehaviour {
        private NavMeshAgent agent;
        private Animator animator;
        private Vector2 smoothDeltaPosition = Vector2.zero;
        private Vector2 velocity = Vector2.zero;
        private float lastKnownDirection;

        // Start is called before the first frame update
        void Start() {
            agent = GetComponentInParent<NavMeshAgent>();
            animator = GetComponent<Animator>();
        }

        private void Update() {
            Vector3 worldDeltaPosition = agent.nextPosition - agent.gameObject.transform.position;

            // Map 'worldDeltaPosition' to local space
            float dx = Vector3.Dot(agent.gameObject.transform.right, worldDeltaPosition);
            float dy = Vector3.Dot(agent.gameObject.transform.forward, worldDeltaPosition);
            Vector2 deltaPosition = new Vector2(dx, dy);

            // Low-pass filter the deltaMove
            float smooth = Mathf.Min(1.0f, Time.deltaTime / 0.15f);
            smoothDeltaPosition = Vector2.Lerp(smoothDeltaPosition, deltaPosition, smooth);

            // Update velocity if time advances
            if (Time.deltaTime > 1e-5f)
                velocity = smoothDeltaPosition / Time.deltaTime;

            bool shouldMove = velocity.magnitude > 0.1f && agent.remainingDistance > agent.radius;

            // update last known direction for idle animations
            if (deltaPosition.x > 0 && deltaPosition.y < 0) {
                lastKnownDirection = 0; // facing NE
            } else if (deltaPosition.x < 0 && deltaPosition.y > 0) {
                lastKnownDirection = 1; // facing SW
            } else if (deltaPosition.x < 0 && deltaPosition.y < 0) {
                lastKnownDirection = 2; // facing NW
            } else if (deltaPosition.x > 0 && deltaPosition.y > 0) {
                lastKnownDirection = 3; // facing SE
            }

            //Debug.Log("AIAnimMoveSync Called: move(" + shouldMove + "), velx(" + velocity.x + "), vely(" + velocity.y + "), lastDirection(" + lastKnownDirection + ")");

            // Update animation parameters
            animator.SetBool("move", shouldMove);
            animator.SetFloat("velx", velocity.x);
            animator.SetFloat("vely", velocity.y);
            animator.SetFloat("lastDirection", lastKnownDirection);
        }

        void OnAnimatorMove() {
            // Update position to agent position
            transform.parent.transform.position = agent.nextPosition;
        }
    }
}

