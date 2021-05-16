using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    public class StateMachineMultiCondition {
        private State currentState;
        private MonoBehaviour monoBehaviour;
        private Dictionary<State, List<Transition>> transitionDict = new Dictionary<State, List<Transition>>();
        private List<Transition> currentTransitions = new List<Transition>();
        private List<Transition> anyTransitions = new List<Transition>();
        private static List<Transition> emptyTransitions = new List<Transition>(0);
        private List<Transition> viableTransitions = new List<Transition>();
        private Transition highestPriorityTransition;
        private bool thinking;
        private bool conditionsMet;
        public bool debugLog;
        private WaitForSeconds thinkPauseTime = new WaitForSeconds(0.5f);

        public void Tick() {
            if (!thinking) {
                if (currentState.isInterruptable) {
                    Transition transition = GetTransition();

                    if (transition != null) {
                        if (transition.to.priority > currentState.priority) {
                            thinking = true;
                            monoBehaviour.StartCoroutine(ThinkPause(transition));
                        } else if (currentState.isFinished) {
                            thinking = true;
                            monoBehaviour.StartCoroutine(ThinkPause(transition));
                        }
                    }
                } else if (currentState.isFinished) {
                    Transition transition = GetTransition();

                    if (transition != null) {
                        thinking = true;
                        monoBehaviour.StartCoroutine(ThinkPause(transition));
                    }
                }

                if (!thinking) {
                    currentState?.Tick();
                }
            }
        }

        public void SetState(State state) {
            if (state == currentState)
                return;

            currentState?.OnExit();
            currentState = state;

            transitionDict.TryGetValue(currentState, out currentTransitions);
            if (currentTransitions == null)
                currentTransitions = emptyTransitions;

            currentState.OnEnter();
        }

        public void AddTransition(State from, State to, List<Func<bool>> conditions) {
            if (transitionDict.TryGetValue(from, out var transitionList) == false) {
                transitionList = new List<Transition>();
                transitionDict[from] = transitionList;
            }

            transitionList.Add(new Transition(to, conditions));
        }

        public void AddToAnyTransition(State from) {
            List<State> stateBuffer = new List<State>();

            foreach (State to in transitionDict.Keys) {
                if (!to.Equals(from)) {
                    stateBuffer.Add(to);
                }
            }

            foreach (State to in stateBuffer) {
                foreach (Transition transition in transitionDict[to]) {
                    AddTransition(from, to, transition.conditions);
                }
            }
        }

        public void AddFromAnyTransition(State state, List<Func<bool>> conditions) {
            anyTransitions.Add(new Transition(state, conditions));
        }

        private class Transition {
            public List<Func<bool>> conditions { get; }
            public State to { get; }

            public Transition(State to, List<Func<bool>> conditions) {
                this.to = to;
                this.conditions = conditions;
            }
        }

        private Transition GetTransition() {
            // Start with fresh transition list
            viableTransitions.Clear();
            highestPriorityTransition = null;

            // Check specific transitions (higher priority with same priority number)
            foreach (Transition transition in currentTransitions) {
                conditionsMet = true; //start by assuming all true

                foreach (Func<bool> condition in transition.conditions) {
                    if (!condition()) {
                        conditionsMet = false;
                        break; // break on first false condition
                    }
                }

                if (conditionsMet) {
                    viableTransitions.Add(transition);
                }
            }

            // Check transitions to non-specific transition
            foreach (Transition transition in anyTransitions) {
                if (!transition.to.Equals(currentState)) {
                    conditionsMet = true; //start by assuming all true

                    foreach (Func<bool> condition in transition.conditions) {
                        if (!condition()) {
                            conditionsMet = false;
                            break; // break on first false condition
                        }
                    }

                    if (conditionsMet) {
                        viableTransitions.Add(transition);
                    }
                }
            }

            foreach (Transition transition in viableTransitions) {
                if (highestPriorityTransition == null) {
                    highestPriorityTransition = transition;
                    continue;
                }

                if (transition.to.priority > highestPriorityTransition.to.priority) {
                    highestPriorityTransition = transition;
                }
            }

            return highestPriorityTransition;
        }

        public void MonoParser(MonoBehaviour monoBehaviour) {
            this.monoBehaviour = monoBehaviour;
        }

        IEnumerator ThinkPause(Transition transition) {
            yield return thinkPauseTime;
            thinking = false;
            SetState(transition.to);

            if (debugLog) {
                Debug.Log("Changing State To: " + transition.to);
            }
        }
    }
}

