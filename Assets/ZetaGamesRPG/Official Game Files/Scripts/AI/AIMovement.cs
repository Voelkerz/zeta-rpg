using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

namespace ZetaGames.RPG {
    public class AIMovement : AILerp {

        private static readonly int animShouldMove = Animator.StringToHash("AnimShouldMove");
        private static readonly int animMoveX = Animator.StringToHash("AnimMoveX");
        private static readonly int animMoveY = Animator.StringToHash("AnimMoveY");
        private static readonly int animLastMoveX = Animator.StringToHash("AnimLastMoveX");
        private static readonly int animLastMoveY = Animator.StringToHash("AnimLastMoveY");
        private Animator animator;
        private MapManager mapManager;
        private float baseSpeed = 2;
        Vector3 direction, previousDirection;

        protected override void Awake() {
            base.Awake();
            animator = GetComponentInChildren<Animator>();
        }

        protected override void Start() {
            base.Start();
            mapManager = FindObjectOfType<MapManager>();
            baseSpeed = speed;
            isStopped = true;
        }

        public override void OnTargetReached() {
            //Debug.Log("OnTargetReached() called");
            isStopped = true;
        }

        public override void SearchPath() {
            base.SearchPath();
            isStopped = false;
        }

        protected override void Update() {
            base.Update();
            
            if (!isStopped && path != null && remainingDistance > 0.65) {
                mapManager.GetGrid().GetXY(transform.position, out int x, out int y);
                Vector3Int gridPos = new Vector3Int(x, y, 0);

                WorldTile currentTile = mapManager.GetWorldTile(gridPos);

                if (currentTile != null) {
                    speed = baseSpeed * (currentTile.speedPercent / 100);
                }

                CalculateNextPosition(out direction, Time.deltaTime);

                previousDirection = direction;

                animator.SetFloat(animMoveX, direction.normalized.x);
                animator.SetFloat(animMoveY, direction.normalized.y);
                animator.SetBool(animShouldMove, !isStopped);
            } else {
                animator.SetBool(animShouldMove, !isStopped);
                animator.SetFloat(animLastMoveX, previousDirection.normalized.x);
                animator.SetFloat(animLastMoveY, previousDirection.normalized.y);
            }
        }
    }
}
