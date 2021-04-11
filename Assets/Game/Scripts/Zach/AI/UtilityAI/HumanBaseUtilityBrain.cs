using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

namespace ZetaGames.RPG {
	public class HumanBaseUtilityBrain : MonoBehaviour {

		public UAI_PropertyBoundedFloat energy, hunger, security, unsure, targetDistance;
		public UAI_PropertyBoundedInt numWood, numCoins;
		public UAI_PropertyBoolean shouldMove;
		private UAI_Agent agent;
		private NavMeshAgent navMeshAgent;
		private AnimationManager animationManager;

		// temp values
		//private Vector3 destination;
		private Vector3 lastPosition;
		private bool atDestination = false;
		public bool moving = false; [HideInInspector]
		public GameObject importantTarget;
		public GameObject lastTarget;
		public bool needWood = true;
		public int numWoodReq = 5;
		public bool hasBuildProject;

		void Start() {
			// GET UTILITY AI AGENT
			agent = GetComponent<UAI_Agent>();

			// GET NAVMESH AGENT
			navMeshAgent = GetComponent<NavMeshAgent>();

			// GET ANIMATION MANAGER
			animationManager = GetComponentInChildren<AnimationManager>();

			// SET STARTING VARIABLES
			lastPosition = transform.position;

			/*********************************
			 * ACTION LIST:
			 * -EvaluateInventory (determines resource count/need)
			 * -Search(gain target)
			 * -Harvest(target)
			 * -Wander
			 * -MoveTo(target)
			 * -Pickup(target)
			***********************************/

			//add function delegate to action
			agent.SetVoidActionDelegate("MoveTo", MoveTo);
			agent.SetVoidActionDelegate("Evaluate Inventory", EvaluateInventory);
			agent.SetVoidActionDelegate("Search", Search);
			agent.SetVoidActionDelegate("Harvest", Harvest);
			agent.SetVoidActionDelegate("Pickup", Pickup);
			agent.SetVoidActionDelegate("Wander", Wander);
		}

		// Update is called once per frame
		void Update() {
			agent.UpdateAI();
		}

		void ResetPositions() {
			if (agent.newAction) {
				moving = false;
				atDestination = false;
				agent.newAction = false;
			}
		}

		// ACTION FUNCTIONS -- TICK EVERY UPDATE()
		void MoveTo() {
            if (importantTarget != null || !atDestination) {
				// Am I stuck?
				if (Vector3.Distance(transform.position, lastPosition) <= 0.0f) {
					unsure.value += 10.0f * UtilityTime.time;
				}

				// Update destination distance consideration
				targetDistance.value = Vector3.Distance(transform.position, importantTarget.transform.position);
				
				if (!moving) {
					navMeshAgent.destination = importantTarget.transform.position;
					moving = true;
					agent.StartTimer();
				}

				lastPosition = transform.position;

                if (shouldMove) {
					animationManager.Move();
				}
				
				if (Vector3.Distance(transform.position, importantTarget.transform.position) < 2f) {;
					shouldMove.value = false;
					atDestination = true;
				}
			} else {
				shouldMove.value = false;
				moving = false;
            }
		}

		void Wander() {
			ResetPositions();
			unsure.value -= 10.0f * UtilityTime.time;
		}

		void Search() {
			ResetPositions();

			agent.StartTimer();

			if (needWood) {
				Collider2D targetCollider = ZetaUtilities.FindNearestCollider(transform, ResourceType.Wood.ToString(), 100f, 1 << 6);
                if (targetCollider != null) {
					importantTarget = targetCollider.gameObject;
				} else {
					importantTarget = null;
                }
				
				if (importantTarget != null) {
					shouldMove.value = true;
				} else {
					importantTarget = ZetaUtilities.FindNearestCollider(transform, ResourceType.Wood.ToString(), 100f, 1 << 7).gameObject;
					if (importantTarget != null) {
						shouldMove.value = true;
					}
				}
			}
		}

		void Harvest() {
			numWood.value =+ 1;
		}

		void Pickup() {

		}

		void EvaluateInventory() {
			/*
			if (!taskManager.taskList.Contains(chopTask)) {
				if (inventory[ResourceType.Wood] < amountOfWoodToCollect) {
					lookingForWood = true;
					Debug.Log("I'm looking for wood");
				}
			} else {
				lookingForWood = false;
			}
			*/
		}
	}
}

