using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Notes
// 1. What a finite state machine is
// 2. Examples where you'd use one
//     AI, Animation, Game State
// 3. Parts of a State Machine
//     States & Transitions
// 4. States - 3 Parts
//     Tick - Why it's not Update()
//     OnEnter / OnExit (setup & cleanup)
// 5. Transitions
//     Separated from states so they can be re-used
//     Easy transitions from any state

namespace ZetaGames.RPG {
    public class StateMachine {
        private IState currentState;
        private MonoBehaviour monoBehaviour;
        private Dictionary<Type, List<Transition>> transitionDict = new Dictionary<Type, List<Transition>>();
        private List<Transition> currentTransitions = new List<Transition>();
        private List<Transition> anyTransitions = new List<Transition>();
        private static List<Transition> emptyTransitions = new List<Transition>(0);
        private bool thinking = false;

        public void Tick() {
            var transition = GetTransition();
            
            if (transition != null && !thinking) {
                thinking = true;
                monoBehaviour.StartCoroutine(ThinkPause(transition));
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

            transitionDict.TryGetValue(currentState.GetType(), out currentTransitions);
            if (currentTransitions == null)
                currentTransitions = emptyTransitions;

            currentState.OnEnter();
        }

        public void AddTransition(IState from, IState to, Func<bool> predicate) {
            if (transitionDict.TryGetValue(from.GetType(), out var transitionList) == false) {
                transitionList = new List<Transition>();
                transitionDict[from.GetType()] = transitionList;
            }

            transitionList.Add(new Transition(to, predicate));
        }

        public void AddAnyTransition(IState state, Func<bool> predicate) {
            anyTransitions.Add(new Transition(state, predicate));
        }

        private class Transition {
            public Func<bool> Condition { get; }
            public IState To { get; }

            public Transition(IState to, Func<bool> condition) {
                To = to;
                Condition = condition;
            }
        }

        private Transition GetTransition() {
            foreach (var transition in anyTransitions)
                if (transition.Condition())
                    return transition;

            foreach (var transition in currentTransitions)
                if (transition.Condition())
                    return transition;

            return null;
        }

        public void MonoParser(MonoBehaviour monoBehaviour) {
            this.monoBehaviour = monoBehaviour;
        }

        IEnumerator ThinkPause(Transition transition) {
            yield return new WaitForSeconds(1f);
            thinking = false;
            SetState(transition.To);
            Debug.Log("Changing State To: " + transition.To);
        }
    }
}

