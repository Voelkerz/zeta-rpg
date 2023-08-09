using System.Collections;
using UnityEngine;

namespace ZetaGames.RPG {
    public class TimeManager : MonoBehaviour {

        public static TimeManager Instance;
        private WaitForSeconds timeScale;
        public int currentTick;

        private void Start() {
            timeScale = new WaitForSeconds(1);
            StartCoroutine(TimePassage());
        }

        private IEnumerator TimePassage() {
            while (true) {
                // One real second = one game tick ( 1440 ticks = 24 real mins)
                currentTick += 1;
                if (currentTick > 1440) {
                    // Midnight, everything resets to zero
                    currentTick = 0;
                }

                yield return timeScale;
            }
        }
    }
}
