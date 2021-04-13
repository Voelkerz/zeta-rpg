using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    public class StateMachineMultiCondition {
        private IState currentState;
        private MonoBehaviour monoBehaviour;
        private Dictionary<IState, List<Transition>> transitionDict = new Dictionary<IState, List<Transition>>();
        private List<Transition> currentTransitions = new List<Transition>();
        private List<Transition> anyTransitions = new List<Transition>();
        private static List<Transition> emptyTransitions = new List<Transition>(0);
        private bool thinking;
        private bool conditionsMet;

        public void Tick() {
            if (currentState.isInterruptable) {
                var transition = GetTransition();

                if (transition != null && !thinking) {
                    thinking = true;
                    monoBehaviour.StartCoroutine(ThinkPause(transition));
                }
            } else if (currentState.isFinished) {
                var transition = GetTransition();

                if (transition != null && !thinking) {
                    thinking = true;
                    monoBehaviour.StartCoroutine(ThinkPause(transition));
                }
            }

            if (!thinking) {
                currentState?.Tick();
            }
        }

        public void SetState(IState state) {
            if (state == currentState)
                return;

            currentState?.OnExit();
            currentState = state;

            transitionDict.TryGetValue(currentState, out currentTransitions);
            if (currentTransitions == null)
                currentTransitions = emptyTransitions;

            currentState.OnEnter();
        }

        public void AddTransition(IState from, IState to, List<Func<bool>> conditions) {
            if (transitionDict.TryGetValue(from, out var transitionList) == false) {
                transitionList = new List<Transition>();
                transitionDict[from] = transitionList;
            }

            transitionList.Add(new Transition(to, conditions));
        }

        public void AddToAnyTransition(IState from) {
            List<IState> stateBuffer = new List<IState>();

            foreach (IState to in transitionDict.Keys) {
                stateBuffer.Add(to);
            }

            foreach (IState to in stateBuffer) {
                foreach (Transition transition in transitionDict[to]) {
                    AddTransition(from, to, transition.conditions);
                }
            }
        }

        public void AddFromAnyTransition(IState state, List<Func<bool>> conditions) {
            anyTransitions.Add(new Transition(state, conditions));
        }

        private class Transition {
            //TODO: should each transition have a priority?
            public List<Func<bool>> conditions { get; }
            public IState to { get; }
            public int priority { get; }

            public Transition(IState to, List<Func<bool>> conditions) {
                this.to = to;
                this.priority = 1; //not implemented
                this.conditions = conditions;
            }
        }

        private Transition GetTransition() {
            //TODO: Sort possible transition based on action priority, if multiple transitions are true

            foreach (Transition transition in anyTransitions) {
                conditionsMet = true; //start by assuming all true

                foreach (Func<bool> condition in transition.conditions) {
                    if (!condition()) {
                        conditionsMet = false;
                        break; // break on first false condition
                    }
                }

                if (conditionsMet) {
                    return transition;
                }
            }

            foreach (Transition transition in currentTransitions) {
                conditionsMet = true; //start by assuming all true

                foreach (Func<bool> condition in transition.conditions) {
                    if (!condition()) {
                        conditionsMet = false;
                        break; // break on first false condition
                    }
                }

                if (conditionsMet) {
                    return transition;
                }
            }

            return null;
        }

        public void MonoParser(MonoBehaviour monoBehaviour) {
            this.monoBehaviour = monoBehaviour;
        }

        IEnumerator ThinkPause(Transition transition) {
            yield return new WaitForSeconds(0.5f);
            thinking = false;
            SetState(transition.to);
            Debug.Log("Changing State To: " + transition.to);
        }
    }
}

