using UnityEngine;
using UnityEngine.AI;

namespace ZetaGames {
    static class ZetaUtilities {

        public static Collider2D FindNearestCollider(Vector2 position, string tag, float range, int layerMask) {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(position, range, layerMask);
            Collider2D nearestCollider = null;

            float minSqrDistance = Mathf.Infinity;

            for (int i = 0; i < colliders.Length; i++) {
                if (colliders[i].tag == tag) {
                    float sqrDistanceToCenter = (position - (Vector2)colliders[i].transform.position).sqrMagnitude;
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

        public static Vector2 RandomNavSphere(Vector2 origin, float dist, int layermask) {
            Vector2 randDirection = Random.insideUnitSphere * dist;

            randDirection += origin;

            NavMeshHit navHit;

            NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);

            return navHit.position;
        }
    }
}

