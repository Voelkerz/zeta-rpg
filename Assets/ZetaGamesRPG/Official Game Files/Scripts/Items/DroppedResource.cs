using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    public class DroppedResource : MonoBehaviour {
        [SerializeField] private ResourceData resourcecData;

        public void PickUp() {
            gameObject.SetActive(false);
            
            // Adjust tile data that this resource drop resides on
            WorldTile currentTile = MapManager.Instance.GetWorldTileGrid().GetGridObject(transform.position);
            currentTile.occupied = false;
            currentTile.occupiedType = "none";
            currentTile.SetTileObject(null);

            Destroy(gameObject, 0.75f);
        }

        public ResourceData GetResourceData() {
            return resourcecData;
        }
    }
}
