using UnityEngine;

public class CreepDespawn : MonoBehaviour {
    private MoreMountains.TopDownEngine.Health health;
    private void OnTriggerEnter2D(Collider2D collidingObject) {
        if (collidingObject.gameObject.tag == "Enemy") {
            health = collidingObject.GetComponent<MoreMountains.TopDownEngine.Health>();
            health.Kill();
        }
    }
}
