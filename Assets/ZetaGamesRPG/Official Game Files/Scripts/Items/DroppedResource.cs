using UnityEngine;

namespace ZetaGames.RPG {
    public class DroppedResource : MonoBehaviour {
        [SerializeField] private ResourceDropData resourcecData;

        public void PickUp() {
            //Debug.Log("DroppedResource.Pickup() called");
            gameObject.SetActive(false);
            
            // Adjust tile data that this resource drop resides on
            WorldTile currentTile = MapManager.Instance.GetWorldTileGrid().GetGridObject(transform.position);
            currentTile.occupied = false;
            currentTile.occupiedStatus = ZetaUtilities.OCCUPIED_NONE;
            currentTile.tileObjectData = null;
            currentTile.SetTileObject(null);

            Destroy(gameObject, 0.5f);
        }

        public ResourceDropData GetResourceData() {
            return resourcecData;
        }
    }
}
