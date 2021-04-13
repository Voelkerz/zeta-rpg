using TMPro;
using UnityEngine;

namespace ZetaGames.RPG {
    public class GatheredText : MonoBehaviour {
        private TMP_Text _text;

        private void Awake() {
            _text = GetComponent<TMP_Text>();
        }

        private void Start() {
            //GetComponentInParent<AIBrain>().onGatheredChanged += (count) => _text.SetText(count.ToString());
        }
    }
}

