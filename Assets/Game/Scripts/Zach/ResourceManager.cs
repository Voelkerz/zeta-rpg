using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ResourceType {
    Wood,
    Stone,
    Coin,
    Food
}

public class ResourceManager : MonoBehaviour {
    // the full list of NPCs and their personal amount of resources
    public Dictionary<string, Dictionary<ResourceType, int>> npcList = new Dictionary<string, Dictionary<ResourceType, int>>();
    
    // the list that will hold the amount of resources an NPC has
    private Dictionary<ResourceType, int> resourceList;

    private GameObject[] initialNpcList;

    void Start() {
        // find all friendly NPCs in scene and initialize their inventories
        initialNpcList = GameObject.FindGameObjectsWithTag("Friendly");
        foreach(GameObject npc in initialNpcList) {
            npcList.Add(npc.name, initializeNpcResources());
        }
    }

    public void addToNpcList(string name, Dictionary<ResourceType, int> resources) {
        if (!npcList.ContainsKey(name)) {
            npcList.Add(name, resources);
        }
    }

    public Dictionary<ResourceType, int> getNpcResources(string name) {
        if(npcList.ContainsKey(name)) {
            return npcList[name];
        } else { return null; }
    }

    public Dictionary<ResourceType, int> initializeNpcResources() {
        resourceList = new Dictionary<ResourceType, int>();
        resourceList.Add(ResourceType.Coin, 0);
        resourceList.Add(ResourceType.Wood, 0);
        resourceList.Add(ResourceType.Stone, 0);
        resourceList.Add(ResourceType.Food, 0);
        return resourceList;
    }
}
