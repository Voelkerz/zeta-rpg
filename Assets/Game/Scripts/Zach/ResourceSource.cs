using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ResourceSource : MonoBehaviour {

    public ResourceType type;
    public int quantity;

    // Events
    public UnityEvent onQuantityChange;

    public void GatherResource(int amount, GameObject gatheringPlayer) {
        quantity -= amount;

        int amountToGive = amount;

        if(quantity < 0) {
            amountToGive = amount + quantity;
        }

        if(quantity <= 0) {
            Destroy(gameObject);
        }

        if(onQuantityChange != null) {
            onQuantityChange.Invoke();
        }
    }
}
