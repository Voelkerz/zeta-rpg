using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    [CreateAssetMenu(menuName = "Game Event", fileName = "New Game Event", order = 51)]
    public class GameEvent : ScriptableObject {

        HashSet<GameEventListener> listeners = new HashSet<GameEventListener>();

        //public static event Action<GameEvent> AnyRaised;

        public void Register(GameEventListener gameEventListener) => listeners.Add(gameEventListener);
        
        public void Deregister(GameEventListener gameEventListener) => listeners.Remove(gameEventListener);

        public void Invoke() {
            foreach (var globalEventListener in listeners) {
                globalEventListener.RaiseEvent();
            }
            //AnyRaised?.Invoke(this);
        }
    }
}

