using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    public class NpcMemory {

        private Dictionary<string, object> memoryBank;

        public NpcMemory() {
            memoryBank = new Dictionary<string, object>();
        }

        public void AddMemory(string key, object memory) {
            if (memoryBank.ContainsKey(key)) {
                memoryBank[key] = memory;
            } else {
                memoryBank.Add(key, memory);
            }
        }

        public object RetrieveMemory(string key) {
            if (memoryBank.TryGetValue(key, out object memory)) {
                return memory;
            } else {
                Debug.LogWarning("RetrieveMemory(): Cannot find key in memory to retrieve.");
                return null;
            }
        }

        public bool ContainsMemory(string key) {
            return memoryBank.ContainsKey(key);
        }

        public void RemoveMemory(string key) {
            if (memoryBank.ContainsKey(key)) {
                memoryBank.Remove(key);
            } else {
                Debug.LogWarning("RetrieveMemory(): Cannot find key in memory to remove.");
            }
        }

        public Dictionary<string, object> GetAllMemories() {
            return memoryBank;
        }
    }
}

