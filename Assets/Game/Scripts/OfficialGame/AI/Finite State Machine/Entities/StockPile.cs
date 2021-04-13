using System;
using TMPro;
using UnityEngine;

//
// Placed on a stockpile scene object and ticks through an animator
// that shows incremental levels of wood being placed down
//

namespace ZetaGames.RPG {
    public class StockPile : MonoBehaviour {

        [SerializeField] private int maxHeld = 12;
        
        private float percent;
        public int gathered;

        private void Awake() {
            gathered = 0;

            gameObject.transform.GetChild(0).gameObject.SetActive(false);
            gameObject.transform.GetChild(1).gameObject.SetActive(false);
            gameObject.transform.GetChild(2).gameObject.SetActive(false);
            gameObject.transform.GetChild(3).gameObject.SetActive(false);
        }

        public void Add() {
            gathered++;

            percent = Mathf.Clamp01((float)gathered / maxHeld);

            if (percent <= .25) {
                if (!gameObject.transform.GetChild(0).gameObject.activeSelf) {
                    gameObject.transform.GetChild(0).gameObject.SetActive(true);
                }
            } else if (percent <= .50) {
                if (!gameObject.transform.GetChild(1).gameObject.activeSelf) {
                    gameObject.transform.GetChild(1).gameObject.SetActive(true);
                }
            } else if (percent <= .75) {
                if (!gameObject.transform.GetChild(2).gameObject.activeSelf) {
                    gameObject.transform.GetChild(2).gameObject.SetActive(true);
                }
            } else {
                if (!gameObject.transform.GetChild(3).gameObject.activeSelf) {
                    gameObject.transform.GetChild(3).gameObject.SetActive(true);
                }
            }
        }
    }
}

