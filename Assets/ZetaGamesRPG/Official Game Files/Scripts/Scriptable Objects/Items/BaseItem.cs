using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    public class BaseItem : ScriptableObject {

        [SerializeField] private string itemName;
        [SerializeField] private string description;
        [SerializeField] private Sprite inventoryIcon;
        [SerializeField] private float weight;

        public string GetItemName() {
            return itemName;
        }

        public string GetItemDescription() {
            return description;
        }

        public float GetItemWeight() {
            return weight;
        }
    }
}

