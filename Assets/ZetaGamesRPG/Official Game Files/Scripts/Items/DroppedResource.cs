using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    public class DroppedResource : MonoBehaviour {
        [SerializeField] private ResourceItemData resourcecData;

        public void PickUp() {
            gameObject.SetActive(false);
            
            // Adjust tile data that this resource drop resides on
            WorldTile currentTile = MapManager.Instance.GetWorldTileGrid().GetGridObject(transform.position);
            currentTile.occupied = false;
            currentTile.occupiedStatus = ZetaUtilities.OCCUPIED_NONE;
            currentTile.SetTileObject(null);

            Destroy(gameObject, 0.5f);
        }

        public ResourceItemData GetResourceData() {
            return resourcecData;
        }
    }
}
