using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ZetaGames.RPG {
    public class GameEventListener : MonoBehaviour {
        [SerializeField] UnityEvent unityEvent;
        [SerializeField] GameEvent gameEvent;

        private void Awake() => gameEvent.Register(this);

        private void OnDestroy() => gameEvent.Deregister(this);

        public void RaiseEvent() => unityEvent.Invoke();
    }
}

