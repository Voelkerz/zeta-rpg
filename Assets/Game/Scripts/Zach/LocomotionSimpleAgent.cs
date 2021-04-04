using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ZetaGames.RPG {
    public class LocomotionSimpleAgent : MonoBehaviour {
        private NavMeshAgent agent;
        private Animator animator;
        private Vector2 smoothDeltaPosition = Vector2.zero;
        private Vector2 velocity = Vector2.zero;

        // Start is called before the first frame update
        void Start() {
            animator = GetComponent<Animator>();
            agent = GetComponent<NavMeshAgent>();
            agent.updateUpAxis = false;
            agent.updateRotation = false;
            agent.updatePosition = false;
        }

        // Update is called once per frame
        void Update() {
            Vector3 worldDeltaPosition = agent.nextPosition - transform.position;

            // Map 'worldDeltaPosition' to local space
            float dx = Vector3.Dot(transform.right, worldDeltaPosition);
            float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
            Vector2 deltaPosition = new Vector2(dx, dy);

            // Low-pass filter the deltaMove
            float smooth = Mathf.Min(1.0f, Time.deltaTime / 0.15f);
            smoothDeltaPosition = Vector2.Lerp(smoothDeltaPosition, deltaPosition, smooth);

            // Update velocity if time advances
            if (Time.deltaTime > 1e-5f)
                velocity = smoothDeltaPosition / Time.deltaTime;

            bool shouldMove = velocity.magnitude > 0.5f && agent.remainingDistance > agent.radius;

            // Update animation parameters
            animator.SetBool("move", shouldMove);
            animator.SetFloat("velx", velocity.x);
            animator.SetFloat("vely", velocity.y);

            //GetComponent<LookAt>().lookAtTargetPosition = agent.steeringTarget + transform.forward;
        }

        void OnAnimatorMove() {
            // Update position to agent position
            transform.parent.transform.position = agent.nextPosition;
        }
    }
}
