using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ZetaGames.RPG {
    public class Wander : IState {
        public bool isFinished { get => finished; }
        public bool isInterruptable { get => npcBrain.inCombat || cycleCount > 3; } // state will not be interrupted until specified full 'wandering' cycles are finished (unless combat is initiated)
        private bool finished;
        private AIBrain npcBrain;
        //private NavMeshAgent navMeshAgent;
        //private readonly AnimationManager animationManager;
        //private Vector3 lastPosition = Vector3.zero;

        public float wanderRadius;
        public float wanderTimer;

        //private Transform target;
        private float timer;
        private int cycleCount;

        public Wander(AIBrain npcBrain, float wanderRadius, float wanderTimer) {
            this.npcBrain = npcBrain;
            //navMeshAgent = npcBrain.navMeshAgent;
            //animationManager = npcBrain.animationManager;
            this.wanderRadius = wanderRadius;
            this.wanderTimer = wanderTimer;
        }

        public void Tick() {
            timer += Time.deltaTime;

            if (timer >= wanderTimer) {
                //Vector2 newPos = ZetaUtilities.RandomNavSphere(npcBrain.transform.position, wanderRadius, -1);
                //navMeshAgent.SetDestination(newPos);
                timer = 0;
                cycleCount++;
            }

            //animationManager.Move();
        }

        public void OnEnter() {
            npcBrain.ResetAgent();
            timer = wanderTimer;
            cycleCount = 0;
            //npcBrain.wanderCooldown = 0;
        }

        public void OnExit() {
           
        }

        /*
        public static Vector2 RandomNavSphere(Vector2 origin, float dist, int layermask) {
            Vector2 randDirection = Random.insideUnitSphere * dist;

            randDirection += origin;

            NavMeshHit navHit;

            NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);

            return navHit.position;
        }
        */
    }
}

