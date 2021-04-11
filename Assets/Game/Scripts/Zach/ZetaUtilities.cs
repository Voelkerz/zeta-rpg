using UnityEngine;

namespace ZetaGames {
    abstract class ZetaUtilities {

        public static Collider2D FindNearestCollider(Transform transform, string tag, float range, int layerMask) {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, range, layerMask);
            Collider2D nearestCollider = null;

            float minSqrDistance = Mathf.Infinity;

            for (int i = 0; i < colliders.Length; i++) {
                if (colliders[i].tag == tag) {
                    float sqrDistanceToCenter = (transform.position - colliders[i].transform.position).sqrMagnitude;
                    if (sqrDistanceToCenter < minSqrDistance) {
                        minSqrDistance = sqrDistanceToCenter;
                        nearestCollider = colliders[i];
                    }
                }
            }

            return nearestCollider;
        }

        public static Vector2 FindNearestEdgeOfCollider(Vector2 position, Collider2D collider) {
            return collider.ClosestPoint(position);
        }
    }
}

