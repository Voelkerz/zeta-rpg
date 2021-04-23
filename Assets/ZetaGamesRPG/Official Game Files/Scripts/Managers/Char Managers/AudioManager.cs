using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    public class AudioManager : MonoBehaviour {
        private AudioSource audioSource;
        public AudioClip[] soundEffects;
        private AudioClip soundClip;

        private void Start() {
            audioSource = gameObject.GetComponentInParent<AudioSource>();
        }

        public void PlaySoundEffect() {
            int index = Random.Range(0, soundEffects.Length);
            audioSource.PlayOneShot(soundEffects[index], 0.8f);
        }
    }
}

