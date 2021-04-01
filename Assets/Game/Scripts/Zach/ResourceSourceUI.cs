using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ResourceSourceUI : MonoBehaviour {

    public GameObject popupPanel;
    public TextMeshProUGUI resourceQuantityText;
    public ResourceSource resource;

    void Start() {
        popupPanel.SetActive(false);
    }

    void OnMouseOver() {
        popupPanel.SetActive(true);
    }

    void OnMouseExit() {
        popupPanel.SetActive(false);
    }
    
    public void OnResourceQuantityChange() {
        resourceQuantityText.text = resource.quantity.ToString();
    }
}
