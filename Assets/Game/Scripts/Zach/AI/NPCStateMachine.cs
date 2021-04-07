using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    public class NPCStateMachine : MonoBehaviour {


        private State _currentState;

        public State state {
            get {
                return _currentState;
            }
            set {
                ExitState(_currentState);
                _currentState = value;
                EnterState(_currentState);
            }
        }

        void ExitState(State state) {
            switch (state) {
                case State.Idle:
                    StopCoroutine("Idle");
                    break;
                case State.EnRoute:
                    StopCoroutine("EnRoute");
                    break;
                case State.Wander:
                    StopCoroutine("Wander");
                    break;
                case State.Explore:
                    StopCoroutine("Explore");
                    break;
                case State.ChopTree:
                    StopCoroutine("ChopTree");
                    break;
                case State.Eat:
                    StopCoroutine("Eat");
                    break;
                case State.Sleep:
                    StopCoroutine("Sleep");
                    break;
                case State.ResourceSearch:
                    StopCoroutine("ResourceSearch");
                    break;
                default:
                    break;
            }

        }

        void EnterState(State state) {
            switch (state) {
                case State.Idle:
                    StartCoroutine("Idle");
                    break;
                case State.EnRoute:
                    StartCoroutine("EnRoute");
                    break;
                case State.Wander:
                    StartCoroutine("Wander");
                    break;
                case State.Explore:
                    StartCoroutine("Explore");
                    break;
                case State.ChopTree:
                    StartCoroutine("ChopTree");
                    break;
                case State.Eat:
                    StartCoroutine("Eat");
                    break;
                case State.Sleep:
                    StartCoroutine("Sleep");
                    break;
                case State.ResourceSearch:
                    StartCoroutine("ResourceSearch");
                    break;
                default:
                    break;
            }
        }
    }
}
