using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetLevelPath : MonoBehaviour {
    private GameObject levelPathObject;
    private MoreMountains.Tools.MMPath levelPath;
    private MoreMountains.Tools.MMPath creeperPath;
    private bool pathUpdated;

    // Start is called before the first frame update
    void Start() {
        levelPathObject = GameObject.FindWithTag("CreeperPath");
        levelPath = levelPathObject.GetComponent<MoreMountains.Tools.MMPath>();
        creeperPath = GetComponent<MoreMountains.Tools.MMPath>();

        pathUpdated = false;
    }

    // Update is called once per frame
    void Update() {
        if (!pathUpdated) {
            creeperPath.PathElements = levelPath.PathElements;
        }
    }
}
