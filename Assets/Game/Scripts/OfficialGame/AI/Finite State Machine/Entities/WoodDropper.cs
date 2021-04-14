using UnityEngine;

namespace ZetaGames.RPG {
    public class WoodDropper : MonoBehaviour {
        // Is put onto a scene object that acts solely as a spawner for the wood drops
        [SerializeField] private HarvestableResource _prefab;
        public void Drop(int gathered, Vector3 position) {
            var resource = Instantiate(_prefab, position, Quaternion.identity);
            //resource.SetHitCountdown(gathered);
        }
    }
}

